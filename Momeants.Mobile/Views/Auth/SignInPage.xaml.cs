using Momeants.Mobile.ViewModels;

namespace Momeants.Mobile.Views.Auth;

public partial class SignInPage : ContentPage
{
    public SignInPage(SignInViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
