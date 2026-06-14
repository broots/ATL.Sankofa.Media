using System.Net.Http.Headers;

namespace ATL.Sankofa.Media.UI.Shared.Auth;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;

    public AuthorizationMessageHandler(ITokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
