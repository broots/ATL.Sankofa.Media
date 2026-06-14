using ATL.Sankofa.Media.Business;
using ATL.Sankofa.Media.UI.Maui.Auth;
using ATL.Sankofa.Media.UI.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;
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
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

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
			handler.InnerHandler = new HttpClientHandler();
			var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";
			return new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
		});
		builder.Services.AddSingleton<TokenAuthStateProvider>();
		builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthStateProvider>());
		builder.Services.AddAuthorizationCore();

		// Register media services
		builder.Services.AddMediaServices(builder.Configuration);

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
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
