import { supabaseAdmin, userFriendlyError } from '../_shared/supabaseClient.mjs';

export async function handler(req) {
  try {
    const body = await req.json();
    const { identifier, password, origen = 'cliente-app' } = body || {};
    if (!identifier || !password) {
      return new Response(JSON.stringify({ message: 'Correo, teléfono o contraseña incorrectos' }), { status: 400 });
    }

    // Decide if identifier is email or phone
    const isEmail = identifier.includes('@');
    let email = null;

    if (isEmail) {
      email = identifier.toLowerCase();
    } else {
      // lookup perfil by telefono
      const { data: perfiles } = await supabaseAdmin.from('perfiles').select('user_id, telefono').eq('telefono', identifier).limit(1);
      if (!perfiles || perfiles.length === 0) {
        return new Response(JSON.stringify({ message: 'No encontramos una cuenta con ese teléfono' }), { status: 404 });
      }
      // fetch auth user to get email (claim it may exist in user metadata)
      const user_id = perfiles[0].user_id;
      const { data: user, error: userErr } = await supabaseAdmin.auth.admin.getUserById(user_id).catch(e => ({ error: e }));
      if (userErr || !user) {
        // Log and return friendly
        await supabaseAdmin.from('logs_errores').insert([{ user_id, origen: 'login-by-phone', evento: 'user_fetch_error', mensaje: 'No se pudo recuperar usuario', detalle: { error: userErr?.message || null } }]);
        return new Response(JSON.stringify({ message: 'No encontramos una cuenta con ese teléfono' }), { status: 404 });
      }
      email = user.email;
      if (!email) {
        return new Response(JSON.stringify({ message: 'No encontramos una cuenta con ese teléfono' }), { status: 404 });
      }
    }

    // sign-in using email+password
    const { data: authResult, error } = await supabaseAdmin.auth.signInWithPassword({ email, password }).catch(e => ({ error: e }));
    if (error || !authResult || !authResult.session) {
      // record failed login
      await supabaseAdmin.from('logs_accesos').insert([{ user_id: null, role: null, origen, evento: 'login_failed', meta: { identifier } }]);
      return new Response(JSON.stringify({ message: 'Correo, teléfono o contraseña incorrectos' }), { status: 401 });
    }

    const user = authResult.user;

    // fetch perfil
    const { data: perfil } = await supabaseAdmin.from('perfiles').select('id, role').eq('user_id', user.id).limit(1);
    const role = perfil && perfil[0] ? perfil[0].role : null;

    // log success
    await supabaseAdmin.from('logs_accesos').insert([{ user_id: user.id, role, origen, evento: 'login_success', meta: { provider: 'password' } }]);

    // return session and safe user info
    return new Response(JSON.stringify({ message: 'Inicio de sesión exitoso', session: authResult.session, user: { id: user.id, email: user.email }, role }), { status: 200 });
  } catch (err) {
    // log error
    await supabaseAdmin.from('logs_errores').insert([{ origen: 'login', evento: 'exception', mensaje: 'Error interno', detalle: { message: err.message } }]);
    return new Response(JSON.stringify({ message: 'No se pudo iniciar sesión' }), { status: 500 });
  }
}
