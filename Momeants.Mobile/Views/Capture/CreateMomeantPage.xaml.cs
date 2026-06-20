using Momeants.Mobile.ViewModels;

namespace Momeants.Mobile.Views.Capture;

public partial class CreateMomeantPage : ContentPage
{
    public CreateMomeantPage(CreateMomeantViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
