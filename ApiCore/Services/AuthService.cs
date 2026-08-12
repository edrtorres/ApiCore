using System.Threading.Tasks;
using ApiCore.Controllers.Models;

namespace ApiCore.Services
{
    public class AuthService : IAuthService
    {
        private readonly ISupabaseAdminClient _supabase;

        public AuthService(ISupabaseAdminClient supabase)
        {
            _supabase = supabase;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest req)
        {
            var created = await _supabase.CreateUserAsync(req.Email, req.Password, req.Phone);
            if (created == null)
            {
                return new AuthResult { Success = false, Message = "No se pudo crear el usuario" };
            }

            // Insert perfil (best-effort)
            var perfil = new { usuario_id = created.GetValueOrDefault("id"), nombre = req.Nombre };
            await _supabase.InsertPerfilAsync(perfil);

            return new AuthResult { Success = true, Message = "Usuario creado. Revisa tu correo para confirmar." };
        }

        public async Task<AuthResult> LoginAsync(LoginRequest req)
        {
            var token = await _supabase.SignInWithPasswordAsync(req.Identifier, req.Password);
            if (token == null) return new AuthResult { Success = false, Message = "Credenciales inválidas" };
            return new AuthResult { Success = true, Message = "Inicio de sesión exitoso", Data = token };
        }

        public async Task<UserInfo?> MeAsync(string accessToken)
        {
            var user = await _supabase.GetUserByAccessTokenAsync(accessToken);
            if (user == null) return null;
            return new UserInfo { Id = user.GetValueOrDefault("id")?.ToString(), Email = user.GetValueOrDefault("email")?.ToString() };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            return await _supabase.RevokeRefreshTokenAsync(refreshToken);
        }
    }
}
