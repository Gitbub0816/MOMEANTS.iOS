using Microsoft.Extensions.Logging;
using Momeants.Mobile.Services.Api;
using Momeants.Mobile.Services.Auth;
using Momeants.Mobile.Services.Haptics;
using Momeants.Mobile.Services.Media;
using Momeants.Mobile.Services.Navigation;
using Momeants.Mobile.Services.Storage;
using Momeants.Mobile.ViewModels;
using Momeants.Mobile.Views.Auth;
using Momeants.Mobile.Views.Capture;
using Momeants.Mobile.Views.Feed;
using Momeants.Mobile.Views.Profile;

namespace Momeants.Mobile;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5000/"
            : "http://localhost:5000/";

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Storage (Keychain)
        builder.Services.AddSingleton<ISecureTokenStore, SecureTokenStore>();

        // Navigation
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // Haptics
        builder.Services.AddSingleton<IHapticsService, HapticsService>();

        // Auth HTTP (unauthenticated)
        builder.Services.AddSingleton<IAuthService>(sp =>
        {
            var tokenStore = sp.GetRequiredService<ISecureTokenStore>();
            var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
            return new AuthService(http, tokenStore);
        });

        // API client (authenticated, with auto-refresh)
        builder.Services.AddSingleton<IApiClient>(sp =>
        {
            var tokenStore = sp.GetRequiredService<ISecureTokenStore>();
            var authService = sp.GetRequiredService<IAuthService>();
            var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
            return new ApiClient(http, tokenStore, authService);
        });

        // Media upload service
        builder.Services.AddSingleton<IMediaUploadService>(sp =>
        {
            var api = sp.GetRequiredService<IApiClient>();
            var http = new HttpClient(); // anonymous for R2
            return new MediaUploadService(api, http);
        });

        // ViewModels
        builder.Services.AddTransient<WelcomeViewModel>();
        builder.Services.AddTransient<SignInViewModel>();
        builder.Services.AddTransient<VerifyCodeViewModel>();
        builder.Services.AddTransient<FeedViewModel>();
        builder.Services.AddTransient<CreateMomeantViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<WelcomePage>();
        builder.Services.AddTransient<SignInPage>();
        builder.Services.AddTransient<VerifyCodePage>();
        builder.Services.AddTransient<FeedPage>();
        builder.Services.AddTransient<CreateMomeantPage>();
        builder.Services.AddTransient<ProfilePage>();

        return builder.Build();
    }
}
