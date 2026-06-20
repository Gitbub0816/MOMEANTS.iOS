using Momeants.Mobile.Services.Api;
using Momeants.Mobile.Services.Auth;
using Momeants.Mobile.Services.Navigation;
using Momeants.Shared.Contracts;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;

    private UserDto? _user;
    public UserDto? User { get => _user; set => SetProperty(ref _user, value); }

    public ICommand LoadCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand EditProfileCommand { get; }

    public ProfileViewModel(IApiClient api, IAuthService auth, INavigationService nav)
    {
        _api = api;
        _auth = auth;
        _nav = nav;

        LoadCommand = new Command(async () => await LoadAsync());
        SignOutCommand = new Command(async () => await SignOutAsync());
        EditProfileCommand = new Command(async () => await _nav.NavigateToAsync("ProfileSetupPage"));
    }

    public async Task InitializeAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            User = await _api.GetAsync<UserDto>("api/users/me");
        }
        finally { IsBusy = false; }
    }

    private async Task SignOutAsync()
    {
        await _auth.LogoutAsync();
        await _nav.NavigateToRootAsync("WelcomePage");
    }
}
