using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.UI.Maui.Auth;
using ATL.Sankofa.Media.UI.Maui.ViewModels;
using ATL.Sankofa.Media.UI.Maui.Views;
using ATL.Sankofa.Media.UI.Shared.Auth;
using ATL.Sankofa.Media.UI.Shared.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ATL.Sankofa.Media.UI.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement(isAndroidForegroundServiceEnabled: false)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Load configuration
        var config = new ConfigurationBuilder()
            .AddJsonStream(GetAppSettingsStream())
            .Build();

        builder.Configuration.AddConfiguration(config);

        // Auth services
        builder.Services.AddSingleton<ITokenStorageService, MauiTokenStorageService>();
        builder.Services.AddSingleton<AuthorizationMessageHandler>();
        builder.Services.AddSingleton(sp =>
        {
            var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
#if DEBUG
            handler.InnerHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
#else
            handler.InnerHandler = new HttpClientHandler();
#endif
            var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";
            return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        });

        // Register HTTP-client-based media services
        builder.Services.AddSingleton<IVideoService>(sp => new VideoServiceClient(sp.GetRequiredService<HttpClient>()));
        builder.Services.AddSingleton<ILiveStreamService>(sp => new LiveStreamServiceClient(sp.GetRequiredService<HttpClient>()));
        builder.Services.AddSingleton<IPaywallService>(sp => new PaywallServiceClient(sp.GetRequiredService<HttpClient>()));
        builder.Services.AddSingleton<ISubscriptionService>(sp => new SubscriptionServiceClient(sp.GetRequiredService<HttpClient>()));

        // ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<WatchViewModel>();

        // Pages
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<WatchPage>();
        builder.Services.AddTransient<TrendingPage>();
        builder.Services.AddTransient<SubscriptionsPage>();
        builder.Services.AddTransient<LibraryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static Stream GetAppSettingsStream()
    {
        var assembly = typeof(MauiProgram).Assembly;
        var stream = assembly.GetManifestResourceStream("ATL.Sankofa.Media.UI.Maui.appsettings.json");
        return stream ?? new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}"));
    }
}
