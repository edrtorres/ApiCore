# cURL examples — SuperEconomico Auth flows

Nota: reemplaza `<EDGE_URL>` por la URL de tu función (ej. `https://<project>.functions.supabase.co/<name>`) y usa `SUPABASE_SERVICE_ROLE_KEY` donde sea necesario para pruebas administrativas.

1) Registro (register)

curl -X POST "<EDGE_URL>/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"juan@example.com","password":"Secret123!","nombre":"Juan Perez","telefono":"+34123456789","role":"cliente","direcciones":[{"alias":"Casa","calle":"Calle Falsa 123"}]}'

Respuesta esperada: { "message": "Registro exitoso. Revisa tu correo para confirmar tu cuenta." }

2) Login (email/password)

curl -X POST "<EDGE_URL>/login" \
  -H "Content-Type: application/json" \
  -d '{"identifier":"juan@example.com","password":"Secret123!","origen":"cliente-app"}'

3) Login por teléfono (request)

curl -X POST "<EDGE_URL>/login-by-phone" \
  -H "Content-Type: application/json" \
  -d '{"phone":"+34123456789","origen":"cliente-app"}'

4) Me (validar sesión)

curl -X GET "<EDGE_URL>/me" \
  -H "Authorization: Bearer <ACCESS_TOKEN>"

5) Logout

curl -X POST "<EDGE_URL>/logout" \
  -H "Content-Type: application/json" \
  -d '{"refresh_token":"<REFRESH_TOKEN>","user_id":"<USER_ID>"}'
