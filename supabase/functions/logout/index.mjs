import { supabaseAdmin } from '../_shared/supabaseClient.mjs';

export async function handler(req) {
  try {
    const body = await req.json();
    const { refresh_token, user_id, origen = 'cliente-app' } = body || {};
    if (!refresh_token && !user_id) return new Response(JSON.stringify({ message: 'No se pudo cerrar la sesión' }), { status: 400 });

    if (refresh_token) {
      // revoke refresh token
      await supabaseAdmin.auth.admin.invalidateRefreshToken(refresh_token).catch(() => {});
    }

    // Log logout
    await supabaseAdmin.from('logs_accesos').insert([{ user_id, evento: 'logout', origen }]);

    return new Response(JSON.stringify({ message: 'Cierre de sesión realizado' }), { status: 200 });
  } catch (err) {
    await supabaseAdmin.from('logs_errores').insert([{ origen: 'logout', evento: 'exception', mensaje: 'Error interno', detalle: { message: err.message } }]);
    return new Response(JSON.stringify({ message: 'No se pudo cerrar la sesión' }), { status: 500 });
  }
}
