using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApiCore.Controllers.Models;
using ApiCore.Core.Interfaces;

namespace ApiCore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var r = await _auth.RegisterAsync(req);
            if (!r.Success) return BadRequest(r);
            return Ok(r);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var r = await _auth.LoginAsync(req);
            if (!r.Success) return Unauthorized(r);
            return Ok(r);
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var auth = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer ")) return Unauthorized();
            var token = auth.Substring("Bearer ".Length);
            var u = await _auth.MeAsync(token);
            if (u == null) return Unauthorized();
            return Ok(u);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] object body)
        {
            // Expect { refresh_token: "..." }
            var refresh = body?.GetType().GetProperty("refresh_token")?.GetValue(body)?.ToString();
            if (string.IsNullOrEmpty(refresh)) return BadRequest(new { message = "refresh_token missing" });
            var ok = await _auth.LogoutAsync(refresh);
            if (!ok) return BadRequest(new { message = "No se pudo cerrar la sesión" });
            return Ok(new { message = "Sesión cerrada" });
        }
    }
}
