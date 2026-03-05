using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace oliejournal.web;

public static class OlieWebCommon
{
    public static async Task ApiDelete(IHttpClientFactory httpClientFactory, HttpContext httpContext, string url, CancellationToken ct)
    {
        using var httpClient = Program.GetOlieBlue(httpClientFactory);

        // Get the access token from the authentication context
        var accessToken = await httpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrEmpty(accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        using var httpResponseMessage = await httpClient.DeleteAsync(url, ct);

        httpResponseMessage.EnsureSuccessStatusCode();
    }

    public static string? UserName(ClaimsPrincipal principal) => ReadClaim(principal, "name");

    private static string? ReadClaim(ClaimsPrincipal principal, string key)
    {
        var user = principal.Identity as ClaimsIdentity;

        string? value = null;

        if (user is not null)
        {
            value = user.Claims.FirstOrDefault(c => c.Type == key)?.Value;
        }

        return value;
    }
}
