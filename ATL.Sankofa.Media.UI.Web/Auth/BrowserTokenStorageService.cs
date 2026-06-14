using ATL.Sankofa.Media.UI.Shared.Auth;
using Microsoft.JSInterop;

namespace ATL.Sankofa.Media.UI.Web.Auth;

public class BrowserTokenStorageService : ITokenStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserTokenStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "auth_token");
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "refresh_token");
    }

    public async Task SetTokensAsync(string token, string refreshToken)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "auth_token", token);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refresh_token", refreshToken);
    }

    public async Task ClearTokensAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "auth_token");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refresh_token");
    }
}
