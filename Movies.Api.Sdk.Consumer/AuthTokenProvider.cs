using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using Identity.Api;

namespace Movies.Api.Sdk.Consumer;

public class AuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private string _cachedToken = string.Empty;
    private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken))
        {
            JwtSecurityToken? jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);
            string expiryTimeText = jwt.Claims.Single(claim => claim.Type == "exp").Value;
            DateTime expiryDateTime = UnixTimeStampToDateTime(int.Parse(expiryTimeText));
            if (expiryDateTime > DateTime.UtcNow)
            {
                return _cachedToken;
            }
        }

        await Lock.WaitAsync();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("https://localhost:5003/token", new TokenGenerationRequest
        {
            UserId = Guid.Parse("d8566de3-b1a6-4a9b-b842-8e3887a82e41"),
            Email = "nick@nickchapsas.com",
            CustomClaims = new Dictionary<string, object>()
            {
                { "admin", true },
                { "trusted_member", true }
            }
        });
        
        string newToken = await response.Content.ReadAsStringAsync();
        _cachedToken = newToken;
        Lock.Release();
        return newToken;
    }

    private static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }
}