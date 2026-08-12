import { supabaseAdmin } from '../_shared/supabaseClient.mjs';

export async function handler(req) {
  try {
    const body = await req.json();
    const { email, password, nombre, telefono, role = 'cliente', direcciones = [], origen = 'cliente-app' } = body || {};
    if (!email || !password || !nombre) {
      return new Response(JSON.stringify({ message: 'Completa los campos requeridos' }), { status: 400 });
    }

    // Create user via admin API
    const { data: createdUser, error: createErr } = await supabaseAdmin.auth.admin.createUser({
      email: email.toLowerCase(),
      password,
      user_metadata: { nombre, telefono }
    }).catch(e => ({ error: e }));

    if (createErr || !createdUser) {
      await supabaseAdmin.from('logs_errores').insert([{ origen: 'register', evento: 'create_user_failed', mensaje: 'No se pudo crear usuario', detalle: { error: createErr?.message } }]);
      return new Response(JSON.stringify({ message: 'No se pudo crear la cuenta, intenta nuevamente' }), { status: 500 });
    }

    const userId = createdUser.id;

    // Insert perfil
    await supabaseAdmin.from('perfiles').insert([{ user_id: userId, role, nombre, telefono }]);

    // Insert addresses if any
    if (Array.isArray(direcciones) && direcciones.length > 0) {
      const inserts = direcciones.map(d => ({ perfil_id: null, alias: d.alias || null, direccion: d }));
      // need perfil id
      const { data: perfilRow } = await supabaseAdmin.from('perfiles').select('id').eq('user_id', userId).limit(1);
      const perfilId = perfilRow && perfilRow[0] ? perfilRow[0].id : null;
      if (perfilId) {
        const dirInserts = direcciones.map(d => ({ perfil_id: perfilId, alias: d.alias || null, direccion: d }));
        await supabaseAdmin.from('direcciones').insert(dirInserts);
      }
    }

    // Log registration
    await supabaseAdmin.from('logs_accesos').insert([{ user_id: userId, role, origen, evento: 'register' }]);

    // Rely on Supabase's email confirm flow; instruct frontend to show friendly message
    return new Response(JSON.stringify({ message: 'Registro exitoso. Revisa tu correo para confirmar tu cuenta.' }), { status: 201 });
  } catch (err) {
    await supabaseAdmin.from('logs_errores').insert([{ origen: 'register', evento: 'exception', mensaje: 'Error en registro', detalle: { message: err.message } }]);
    return new Response(JSON.stringify({ message: 'No se pudo crear la cuenta, intenta nuevamente' }), { status: 500 });
  }
}
