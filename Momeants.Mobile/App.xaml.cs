using Momeants.Mobile.Services.Auth;
using Momeants.Mobile.Services.Storage;

namespace Momeants.Mobile;

public partial class App : Application
{
    private readonly ISecureTokenStore _tokenStore;
    private readonly IAuthService _authService;

    public App(ISecureTokenStore tokenStore, IAuthService authService)
    {
        InitializeComponent();
        _tokenStore = tokenStore;
        _authService = authService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await RouteOnStartup();
    }

    private async Task RouteOnStartup()
    {
        try
        {
            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshed = await _authService.TryRefreshAsync();
                if (refreshed)
                {
                    await Shell.Current.GoToAsync("//main/FeedPage");
                    return;
                }
            }
        }
        catch { }

        await Shell.Current.GoToAsync("//WelcomePage");
    }
}
