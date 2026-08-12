import { supabaseAdmin } from '../_shared/supabaseClient.mjs';

// process-auth: used for handling email confirmation redirects and password recover
export async function handler(req) {
  try {
    const url = new URL(req.url);
    const next = url.searchParams.get('next') || '/';
    const type = url.searchParams.get('type'); // e.g., 'recovery' or 'signup'
    const token = url.searchParams.get('token');
    const token_hash = url.searchParams.get('token_hash');

    // For security, just redirect to client with friendly messages.
    // Client should call /me after redirect to verify.
    const redirectUrl = new URL(next);
    if (type === 'recovery') {
      redirectUrl.searchParams.set('message', 'password_reset');
    } else if (type === 'signup') {
      redirectUrl.searchParams.set('message', 'email_confirmed');
    }

    // record event
    await supabaseAdmin.from('logs_accesos').insert([{ origen: 'process-auth', evento: 'redirect', meta: { type } }]);

    return Response.redirect(redirectUrl.toString(), 302);
  } catch (err) {
    await supabaseAdmin.from('logs_errores').insert([{ origen: 'process-auth', evento: 'exception', mensaje: 'Error interno', detalle: { message: err.message } }]);
    return new Response(JSON.stringify({ message: 'No se pudo procesar la autenticación' }), { status: 500 });
  }
}
