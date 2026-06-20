using Momeants.Mobile.Views.Auth;
using Momeants.Mobile.Views.Capture;

namespace Momeants.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for modal/pushed pages
        Routing.RegisterRoute("SignInPage", typeof(SignInPage));
        Routing.RegisterRoute("VerifyCodePage", typeof(VerifyCodePage));
        Routing.RegisterRoute("CreateAccountPage", typeof(SignInPage)); // reuse sign-in for now
        Routing.RegisterRoute("CreateMomeantPage", typeof(CreateMomeantPage));
        Routing.RegisterRoute("ProfileSetupPage", typeof(SignInPage));
        Routing.RegisterRoute("MomeantDetailPage", typeof(SignInPage));
    }
}
