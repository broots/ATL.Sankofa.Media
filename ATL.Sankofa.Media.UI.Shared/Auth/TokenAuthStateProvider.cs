using System.Net.Http.Json;
using System.Security.Claims;
using ATL.Sankofa.Media.Business.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace ATL.Sankofa.Media.UI.Shared.Auth;

public class TokenAuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly HttpClient _httpClient;

    public TokenAuthStateProvider(ITokenStorageService tokenStorage, HttpClient httpClient)
    {
        _tokenStorage = tokenStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = JwtTokenParser.ParseClaimsFromJwt(token);
        var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        if (expiry != null && long.TryParse(expiry, out var expUnix))
        {
            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expUnix);
            if (expiryDate <= DateTimeOffset.UtcNow)
            {
                // Token expired - try refresh
                var refreshed = await TryRefreshTokenAsync();
                if (!refreshed)
                {
                    await _tokenStorage.ClearTokensAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                token = await _tokenStorage.GetTokenAsync();
                claims = JwtTokenParser.ParseClaimsFromJwt(token!);
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public async Task LoginAsync(AuthResponse authResponse)
    {
        await _tokenStorage.SetTokensAsync(authResponse.Token!, authResponse.RefreshToken!);
        var claims = JwtTokenParser.ParseClaimsFromJwt(authResponse.Token!);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task LogoutAsync()
    {
        await _tokenStorage.ClearTokensAsync();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        var refreshToken = await _tokenStorage.GetRefreshTokenAsync();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            return false;

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest
            {
                Token = token,
                RefreshToken = refreshToken
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result?.Succeeded != true)
                return false;

            await _tokenStorage.SetTokensAsync(result.Token!, result.RefreshToken!);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
