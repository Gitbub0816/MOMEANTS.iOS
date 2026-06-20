using Momeants.Mobile.ViewModels;

namespace Momeants.Mobile.Views.People;

public partial class ImportantPeoplePage : ContentPage
{
    private readonly ImportantPeopleViewModel _vm;

    public ImportantPeoplePage(ImportantPeopleViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }
}
