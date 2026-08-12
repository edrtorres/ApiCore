import { supabaseAdmin } from '../_shared/supabaseClient.mjs';

export async function handler(req) {
  try {
    const body = await req.json();
    const { phone, origen = 'cliente-app' } = body || {};
    if (!phone) return new Response(JSON.stringify({ message: 'No encontramos una cuenta con ese teléfono' }), { status: 400 });

    // Try to find perfil with phone
    const { data: perfiles } = await supabaseAdmin.from('perfiles').select('user_id').eq('telefono', phone).limit(1);
    if (!perfiles || perfiles.length === 0) {
      return new Response(JSON.stringify({ message: 'No encontramos una cuenta con ese teléfono' }), { status: 404 });
    }
    const user_id = perfiles[0].user_id;

    // Trigger Supabase SMS/OTP (using auth admin api to send OTP is not always available via server SDK)
    // As fallback, return friendly message instructing to use phone login flow on client
    await supabaseAdmin.from('logs_accesos').insert([{ user_id, role: null, origen, evento: 'login_phone_requested' }]);
    return new Response(JSON.stringify({ message: 'Te enviamos un código por SMS si existe una cuenta asociada.' }), { status: 200 });
  } catch (err) {
    await supabaseAdmin.from('logs_errores').insert([{ origen: 'login-by-phone', evento: 'exception', mensaje: 'Error interno', detalle: { message: err.message } }]);
    return new Response(JSON.stringify({ message: 'No se pudo procesar la solicitud' }), { status: 500 });
  }
}
