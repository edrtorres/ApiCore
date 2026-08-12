using System.Threading.Tasks;
using ApiCore.Controllers.Models;

namespace ApiCore.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest req);
        Task<AuthResult> LoginAsync(LoginRequest req);
        Task<UserInfo?> MeAsync(string accessToken);
        Task<bool> LogoutAsync(string refreshToken);
    }
}
