using System.Threading.Tasks;
using ApiCore.Controllers.Models;
using ApiCore.Core.Interfaces;
using System.Threading.Tasks;

namespace ApiCore.UseCases
{
    public class AuthService : IAuthService
    {
        private readonly ISupabaseClient _supabase;

        public AuthService(ISupabaseClient supabase)
        {
            _supabase = supabase;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest req)
        {
            var created = await _supabase.CreateUserAsync(req.Email, req.Password, req.Phone);
            if (created == null)
            {
                return new AuthResult(false, "No se pudo crear el usuario");
            }

            var perfil = new ApiCore.Domain.Entities.Perfil
            {
                UsuarioId = created.GetValueOrDefault("id")?.ToString(),
                Nombre = req.Nombre
            };
            await _supabase.InsertPerfilAsync(perfil);

            return new AuthResult(true, "Usuario creado. Revisa tu correo para confirmar.");
        }

        public async Task<AuthResult> LoginAsync(LoginRequest req)
        {
            var token = await _supabase.SignInWithPasswordAsync(req.Identifier, req.Password);
            if (token == null) return new AuthResult(false, "Credenciales inválidas");
            return new AuthResult(true, "Inicio de sesión exitoso", token);
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
