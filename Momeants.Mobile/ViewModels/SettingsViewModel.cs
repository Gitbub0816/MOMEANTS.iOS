using Momeants.Mobile.Services.Api;
using Momeants.Mobile.Services.Auth;
using Momeants.Mobile.Services.Navigation;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;

    private string _profileVisibility = "friends";
    public string ProfileVisibility { get => _profileVisibility; set => SetProperty(ref _profileVisibility, value); }

    public ICommand SaveCommand { get; }
    public ICommand SignOutCommand { get; }

    public SettingsViewModel(IApiClient api, IAuthService auth, INavigationService nav)
    {
        _api = api;
        _auth = auth;
        _nav = nav;
        SaveCommand = new Command(async () => await SaveAsync());
        SignOutCommand = new Command(async () => { await _auth.LogoutAsync(); await _nav.NavigateToRootAsync("WelcomePage"); });
    }

    public async Task InitializeAsync()
    {
        var settings = await _api.GetAsync<dynamic>("api/settings");
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        try { await _api.PatchAsync<object>("api/settings", new { profileVisibility = ProfileVisibility }); }
        finally { IsBusy = false; }
    }
}
