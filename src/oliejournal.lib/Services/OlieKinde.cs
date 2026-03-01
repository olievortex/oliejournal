using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace oliejournal.lib.Services;

[ExcludeFromCodeCoverage]
public class OlieKinde(HttpClient httpClient, IOlieConfig config) : IOlieKinde
{
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task<bool> DeleteUser(string userId, CancellationToken ct)
    {
        await EnsureAccessToken(ct);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        
        var response = await httpClient.DeleteAsync($"https://{config.KindeDomain}/api/v1/user?id={userId}", ct);
        
        return response.IsSuccessStatusCode;
    }

    private async Task EnsureAccessToken(CancellationToken ct)
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://{config.KindeDomain}/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = config.KindeClientId,
                ["client_secret"] = config.KindeClientSecret,
                ["audience"] = $"https://{config.KindeDomain}/api"
            })
        };

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
            ?? throw new ApplicationException("Failed to get Kinde access token");

        _accessToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 1 minute early
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn
    );
}