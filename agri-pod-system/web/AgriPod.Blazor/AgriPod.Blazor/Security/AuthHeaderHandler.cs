using System.Net.Http.Headers;
using AgriPod.Shared;

namespace AgriPod.Blazor.Security;

public sealed class AuthHeaderHandler(PortalAuthState authState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!authState.IsAuthenticated && request.RequestUri?.AbsolutePath != "/api/v1/auth/login")
        {
            await SignInAsDemoAdminAsync(request, cancellationToken);
        }

        if (authState.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task SignInAsDemoAdminAsync(HttpRequestMessage originalRequest, CancellationToken cancellationToken)
    {
        var baseUri = originalRequest.RequestUri is null
            ? new Uri("http://localhost:5068")
            : new Uri(originalRequest.RequestUri.GetLeftPart(UriPartial.Authority));

        using var client = new HttpClient { BaseAddress = baseUri };
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("admin@agripod.local", "demo"), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>(cancellationToken);
        if (token is not null)
        {
            authState.SignIn("admin@agripod.local", token.Role, token.AccessToken, token.ExpiresAt);
        }
    }
}
