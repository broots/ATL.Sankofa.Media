using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using ATL.Sankofa.Media.UI.Web;
using ATL.Sankofa.Media.UI.Web.Auth;
using ATL.Sankofa.Media.UI.Shared.Services;
using ATL.Sankofa.Media.UI.Shared.Auth;
using ATL.Sankofa.Media.Business.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// MudBlazor
builder.Services.AddMudServices();

// Token storage and auth state
builder.Services.AddScoped<ITokenStorageService, BrowserTokenStorageService>();
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped<TokenAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// HTTP client with auth handler pointing to the host API
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
});

// Register HTTP client service implementations
builder.Services.AddScoped<IVideoService, VideoServiceClient>();
builder.Services.AddScoped<ILiveStreamService, LiveStreamServiceClient>();
builder.Services.AddScoped<IChannelService, ChannelServiceClient>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionServiceClient>();
builder.Services.AddScoped<IPaywallService, PaywallServiceClient>();

await builder.Build().RunAsync();
