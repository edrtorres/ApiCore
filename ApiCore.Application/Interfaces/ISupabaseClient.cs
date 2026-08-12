using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApiCore.Application.Interfaces
{
    public interface ISupabaseClient
    {
        Task<Dictionary<string, object>?> CreateUserAsync(string email, string password, string? phone = null);
        Task<Dictionary<string, object>?> SignInWithPasswordAsync(string emailOrPhone, string password);
        Task<Dictionary<string, object>?> GetUserByAccessTokenAsync(string accessToken);
        Task<bool> InsertPerfilAsync(object perfil);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
