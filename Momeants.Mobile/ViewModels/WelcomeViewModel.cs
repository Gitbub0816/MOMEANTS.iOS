using Momeants.Mobile.Services.Navigation;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public class WelcomeViewModel : BaseViewModel
{
    private readonly INavigationService _nav;

    public ICommand SignInCommand { get; }
    public ICommand CreateAccountCommand { get; }

    public WelcomeViewModel(INavigationService nav)
    {
        _nav = nav;
        SignInCommand = new Command(async () => await _nav.NavigateToAsync("SignInPage"));
        CreateAccountCommand = new Command(async () => await _nav.NavigateToAsync("CreateAccountPage"));
    }
}
