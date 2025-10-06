using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenRefreshMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // 🧩 1. Read tokens from cookies
            var accessToken = context.Request.Cookies["AuthToken"];
            var refreshToken = context.Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                await _next(context);
                return;
            }

            // 🧩 2. Parse the JWT and check expiry
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt;

            try
            {
                jwt = handler.ReadJwtToken(accessToken);
            }
            catch
            {
                // Invalid token format, skip refresh
                await _next(context);
                return;
            }

            var expires = jwt.ValidTo;

            // 🧩 3. If the token is expiring within a minute, try refresh
            if (expires <= DateTime.UtcNow.AddMinutes(1) && !string.IsNullOrEmpty(refreshToken))
            {
                var client = _httpClientFactory.CreateClient();

                var refreshData = new { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(refreshData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var refreshUrl = $"{context.Request.Scheme}://{context.Request.Host}/Auth/RefreshToken";

                var response = await client.PostAsync(refreshUrl, content);

                // 🧩 4. If successful, read new tokens
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (tokenResponse is not null)
                    {
                        // 🧩 5. Update cookies with new tokens
                        context.Response.Cookies.Append("AuthToken", tokenResponse.AccessToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddMinutes(30)
                        });

                        context.Response.Cookies.Append("RefreshToken", tokenResponse.RefreshToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(7)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 🧩 6. Log error (optional) but never crash the request
            Console.WriteLine($"Token refresh middleware error: {ex.Message}");
        }

        // 🧩 7. Always continue pipeline
        await _next(context);
    }
}

// DTO for deserializing token refresh response
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
