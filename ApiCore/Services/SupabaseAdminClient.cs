using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApiCore.Services
{
    public class SupabaseAdminClient : ISupabaseAdminClient
    {
        private readonly HttpClient _http;
        private readonly string _serviceKey;

        public SupabaseAdminClient(HttpClient http)
        {
            _http = http;
            _serviceKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY") ?? string.Empty;
            if (!string.IsNullOrEmpty(_serviceKey))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
                _http.DefaultRequestHeaders.Add("apikey", _serviceKey);
            }
        }

        public async Task<Dictionary<string, object>?> CreateUserAsync(string email, string password, string? phone = null)
        {
            var payload = new Dictionary<string, object>
            {
                ["email"] = email,
                ["password"] = password
            };
            if (!string.IsNullOrEmpty(phone)) payload["phone"] = phone;

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("/auth/v1/admin/users", content);
            var s = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<Dictionary<string, object>>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Dictionary<string, object>?> SignInWithPasswordAsync(string emailOrPhone, string password)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = emailOrPhone,
                ["password"] = password
            };

            var content = new FormUrlEncodedContent(form);
            var res = await _http.PostAsync("/auth/v1/token", content);
            var s = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<Dictionary<string, object>>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Dictionary<string, object>?> GetUserByAccessTokenAsync(string accessToken)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/auth/v1/user");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var res = await _http.SendAsync(req);
            var s = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<Dictionary<string, object>>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> InsertPerfilAsync(object perfil)
        {
            var content = new StringContent(JsonSerializer.Serialize(perfil), Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Post, "/rest/v1/perfiles") { Content = content };
            req.Headers.Add("Prefer", "return=representation");
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var payload = new Dictionary<string, string> { ["refresh_token"] = refreshToken };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync("/auth/v1/logout", content);
            return res.IsSuccessStatusCode;
        }
    }
}
