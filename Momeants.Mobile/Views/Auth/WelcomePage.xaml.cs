using Momeants.Mobile.ViewModels;

namespace Momeants.Mobile.Views.Auth;

public partial class WelcomePage : ContentPage
{
    public WelcomePage(WelcomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
