using Momeants.Mobile.Services.Api;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public record NotificationItem(Guid Id, string Type, string Title, string? Body, DateTimeOffset CreatedAt, bool IsRead);

public class NotificationsViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    public ObservableCollection<NotificationItem> Items { get; } = new();
    public ICommand LoadCommand { get; }
    public ICommand MarkAllReadCommand { get; }

    public NotificationsViewModel(IApiClient api)
    {
        _api = api;
        LoadCommand = new Command(async () => await LoadAsync());
        MarkAllReadCommand = new Command(async () => await MarkAllReadAsync());
    }

    public async Task InitializeAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _api.GetAsync<List<dynamic>>("api/notifications");
            Items.Clear();
            // Dynamic deserialization — in production use typed DTO
        }
        finally { IsBusy = false; }
    }

    private async Task MarkAllReadAsync()
        => await _api.PostAsync<object>("api/notifications/read-all");
}
