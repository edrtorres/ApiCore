# SuperEconomico — Auth System (Supabase)

Resumen de lo añadido y pasos para desplegar/probar.

Arquitectura:
- Supabase Auth como fuente de identidad.
- Tablas en DB para `perfiles`, `direcciones`, `metodos_pago`, `aceptaciones_login`, `logs_accesos`, `logs_errores`.
- Edge Functions (supabase) que exponen endpoints: `login`, `login-by-phone`, `logout`, `me`, `process-auth`.

Archivos añadidos:
- `supabase/migrations/001_create_auth_and_logs.sql` — migración idempotente para asegurar tablas.
- `supabase/functions/_shared/supabaseClient.mjs` — cliente admin para funciones (usa `SUPABASE_URL` y `SUPABASE_SERVICE_ROLE_KEY` en el entorno).
- `supabase/functions/login/index.mjs` — endpoint para login por email o teléfono (resuelve a email si es teléfono).
- `supabase/functions/login-by-phone/index.mjs` — flujo para login por teléfono (envío de SMS / OTP gestionado por cliente o supabase según configuración).
- `supabase/functions/me/index.mjs` — valida token y retorna `user`, `perfil` y `role`.
- `supabase/functions/logout/index.mjs` — revoca refresh token y registra logout.
- `supabase/functions/process-auth/index.mjs` — punto de entrada para redirects de confirmación/recuperación (redirige al frontend con mensajes amigables).
- `supabase/functions/package.json` — dependencias para funciones (`@supabase/supabase-js`).

Endpoints (edge functions) y uso:
- POST /login
  - body: { identifier, password, origen }
  - `identifier` puede ser email o teléfono. Mensajes de respuesta son amigables.

- POST /login-by-phone
  - body: { phone, origen }
  - intenta localizar perfil por teléfono y dispara flujo SMS/OTP (o indica al cliente que maneje OTP).

- GET /me
  - Header: `Authorization: Bearer <access_token>`
  - Retorna usuario, perfil y role. Frontend usa esto para redirección por rol.

- POST /logout
  - body: { refresh_token, user_id, origen }
  - Revoca refresh token (si se provee) y registra logout.

- GET /process-auth?type=signup|recovery&token=...&next=<deep_link>
  - Usado por `redirect_to` en los correos de Supabase. Redirige al `next` con mensajes claros (ej. `?message=email_confirmed`).

Mensajes de frontend (traducción recomendada):
- Registro exitoso: "Revisa tu correo para confirmar tu cuenta".
- Login fallido: "Correo, teléfono o contraseña incorrectos".
- Cuenta no encontrada (teléfono): "No encontramos una cuenta con ese teléfono".
- Correo no confirmado: "Tu correo no está confirmado".
- Recuperación enviada: "Te enviamos un enlace para restablecer tu contraseña".
- Enlace inválido o vencido: "El enlace no es válido o venció".
- Error genérico: "No se pudo procesar la solicitud, intenta nuevamente".

Notas de integración con frontend Android / cPanel:
- Registro: cliente crea usuario en Supabase Auth (o via Edge Function `register` si prefieres centralizar). Luego insertar fila en `perfiles` con `user_id` retornado.
- Confirmación: en las settings de Supabase Auth poner `redirect_to` apuntando a `https://<tu-frontend>/process-auth` (o a la Edge Function `process-auth`). De esa manera, cuando el usuario confirma, será redirigido al cliente con `?message=email_confirmed`.
- Login: cliente llama a `/login` con `identifier` y `password`. Si éxito, guarda `session.access_token` y `refresh_token` en storage seguro.
- Me: al arrancar, frontend llama `/me` con `Authorization` para validar sesión y redirigir según `role`.
- Recuperación: usar la funcionalidad nativa de Supabase para enviar recovery email, con `redirect_to` apuntando a `process-auth`.

Logging y auditoría:
- Todas las funciones insertan en `logs_accesos` o `logs_errores` según corresponda.
- `logs_accesos` guarda `user_id`, `role`, `origen`, `evento`, `user_agent`, `ip`, `meta`.
- `logs_errores` guarda `mensaje` y `detalle` (json).

Despliegue:
1. Ejecutar la migración SQL en Supabase (SQL editor o `psql`).
2. Configurar variables de entorno en Supabase Functions: `SUPABASE_URL`, `SUPABASE_SERVICE_ROLE_KEY`.
3. Deploy con `supabase functions deploy <name>` o usando `supabase/` folder per tu flujo CI.
4. Actualizar `redirect_to` en Supabase Auth (Signup & Recovery) para apuntar a `https://<tu-frontend>/process-auth` o la ruta de la Edge Function.

Pruebas:
- Registro: llenar formulario y comprobar que:
  - Usuario creado en Auth
  - Fila creada en `perfiles` con role correcto
  - `logs_accesos` no registra error
- Confirmación: usar el link del correo y comprobar redirect y `?message=email_confirmed`.
- Login: probar con email y con teléfono (si teléfono no tiene email asociado, el flujo pedirá registro o recuperación).
- Me: llamar con token y comprobar rol y perfil retornado.
- Recuperación: solicitar reset y comprobar correo enviado y redirect.

Notas finales:
- No modifiqué el código del frontend en este repo (el proyecto .NET es independiente). Estas funciones y migraciones pueden integrarse en tu stack Supabase.
- Si quieres, implemento una función adicional `register` que centralice creación de usuario + perfil + direcciones en una sola llamada (recomendado para evitar race conditions). Dime si la quieres y la implemento.
