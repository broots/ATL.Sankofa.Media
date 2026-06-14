using ATL.Sankofa.Media.UI.Shared.Auth;
using Microsoft.Maui.Storage;

namespace ATL.Sankofa.Media.UI.Maui.Auth;

public class MauiTokenStorageService : ITokenStorageService
{
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";

    public Task<string?> GetTokenAsync()
    {
        var token = SecureStorage.Default.GetAsync(TokenKey).Result;
        return Task.FromResult(token);
    }

    public Task<string?> GetRefreshTokenAsync()
    {
        var token = SecureStorage.Default.GetAsync(RefreshTokenKey).Result;
        return Task.FromResult(token);
    }

    public async Task SetTokensAsync(string token, string refreshToken)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
    }

    public Task ClearTokensAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        return Task.CompletedTask;
    }
}
