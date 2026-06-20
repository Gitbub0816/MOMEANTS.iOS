using Momeants.Mobile.ViewModels;

namespace Momeants.Mobile.Views.Auth;

public partial class VerifyCodePage : ContentPage
{
    public VerifyCodePage(VerifyCodeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
