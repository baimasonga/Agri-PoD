using System.Net.Http.Headers;

namespace AgriPod.Blazor.Security;

public sealed class AuthHeaderHandler(PortalAuthState authState) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (authState.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
