using Momeants.Mobile.Services.Api;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public record ImportantPersonItem(Guid Id, string DisplayName, string Category, string? CustomLabel);

public class ImportantPeopleViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    public ObservableCollection<ImportantPersonItem> Items { get; } = new();
    public ICommand LoadCommand { get; }

    public ImportantPeopleViewModel(IApiClient api)
    {
        _api = api;
        LoadCommand = new Command(async () => await LoadAsync());
    }

    public async Task InitializeAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _api.GetAsync<List<ImportantPersonItem>>("api/important-people");
            Items.Clear();
            if (list is not null) foreach (var p in list) Items.Add(p);
        }
        finally { IsBusy = false; }
    }
}
