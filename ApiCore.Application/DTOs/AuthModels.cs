namespace ApiCore.Application.DTOs
{
    public record RegisterRequest(string Email, string Password, string Nombre, string? Phone);
    public record LoginRequest(string Identifier, string Password);
    public record AuthResult(bool Success = false, string Message = "", object? Data = null);
    public record UserInfo
    {
        public string? Id { get; init; }
        public string? Email { get; init; }
    }
}
