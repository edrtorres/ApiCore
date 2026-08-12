import { supabaseAdmin } from '../_shared/supabaseClient.mjs';

export async function handler(req) {
  try {
    const authHeader = req.headers.get('authorization') || '';
    const token = authHeader.replace('Bearer ', '');
    if (!token) return new Response(JSON.stringify({ message: 'Sesión no válida' }), { status: 401 });

    const { data: userRes, error } = await supabaseAdmin.auth.getUser(token).catch(e => ({ error: e }));
    if (error || !userRes || !userRes.user) {
      return new Response(JSON.stringify({ message: 'Sesión no válida' }), { status: 401 });
    }
    const user = userRes.user;

    // fetch perfil
    const { data: perfil } = await supabaseAdmin.from('perfiles').select('id, role, nombre').eq('user_id', user.id).limit(1);
    const role = perfil && perfil[0] ? perfil[0].role : null;

    return new Response(JSON.stringify({ ok: true, user: { id: user.id, email: user.email }, perfil: perfil && perfil[0] ? perfil[0] : null, role }), { status: 200 });
  } catch (err) {
    await supabaseAdmin.from('logs_errores').insert([{ origen: 'me', evento: 'exception', mensaje: 'Error interno', detalle: { message: err.message } }]);
    return new Response(JSON.stringify({ message: 'No se pudo verificar la sesión' }), { status: 500 });
  }
}
