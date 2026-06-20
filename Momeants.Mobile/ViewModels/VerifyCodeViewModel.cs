using Momeants.Mobile.Services.Auth;
using Momeants.Mobile.Services.Navigation;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

[QueryProperty(nameof(Identifier), "identifier")]
public class VerifyCodeViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;

    private string _identifier = string.Empty;
    public string Identifier
    {
        get => _identifier;
        set => SetProperty(ref _identifier, value);
    }

    private string _code = string.Empty;
    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public ICommand VerifyCommand { get; }
    public ICommand ResendCommand { get; }

    public VerifyCodeViewModel(IAuthService auth, INavigationService nav)
    {
        _auth = auth;
        _nav = nav;
        VerifyCommand = new Command(async () => await OnVerify(), () => !IsBusy);
        ResendCommand = new Command(async () => await OnResend(), () => !IsBusy);
    }

    private async Task OnVerify()
    {
        if (string.IsNullOrWhiteSpace(Code)) { SetError("Please enter the verification code."); return; }
        ClearError();
        IsBusy = true;
        try
        {
            var result = await _auth.VerifyAsync(Identifier, Code.Trim());
            if (result is not null)
                await _nav.NavigateToRootAsync("FeedPage");
            else
                SetError("Invalid or expired code. Please try again.");
        }
        finally { IsBusy = false; }
    }

    private async Task OnResend()
    {
        IsBusy = true;
        try { await _auth.StartAsync(Identifier); }
        finally { IsBusy = false; }
    }
}
