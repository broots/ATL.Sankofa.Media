namespace ATL.Sankofa.Media.UI.Shared.Auth;

public interface ITokenStorageService
{
    Task<string?> GetTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SetTokensAsync(string token, string refreshToken);
    Task ClearTokensAsync();
}
