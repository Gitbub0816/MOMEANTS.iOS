using Momeants.Mobile.Services.Auth;
using Momeants.Mobile.Services.Navigation;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public class SignInViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;

    private string _identifier = string.Empty;
    public string Identifier
    {
        get => _identifier;
        set => SetProperty(ref _identifier, value);
    }

    public ICommand ContinueCommand { get; }

    public SignInViewModel(IAuthService auth, INavigationService nav)
    {
        _auth = auth;
        _nav = nav;
        ContinueCommand = new Command(async () => await OnContinue(), () => !IsBusy);
    }

    private async Task OnContinue()
    {
        if (string.IsNullOrWhiteSpace(Identifier)) { SetError("Please enter your email or phone."); return; }
        ClearError();
        IsBusy = true;
        try
        {
            var ok = await _auth.StartAsync(Identifier.Trim());
            if (ok)
                await _nav.NavigateToAsync("VerifyCodePage", new Dictionary<string, object> { ["identifier"] = Identifier.Trim() });
            else
                SetError("Could not send verification code. Please try again.");
        }
        finally { IsBusy = false; }
    }
}
