using Momeants.Mobile.Services.Api;
using Momeants.Mobile.Services.Haptics;
using Momeants.Mobile.Services.Navigation;
using Momeants.Shared.Contracts;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public class FeedViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IHapticsService _haptics;
    private readonly INavigationService _nav;

    public ObservableCollection<MomeantDto> Items { get; } = new();

    private MomeantDto? _currentItem;
    public MomeantDto? CurrentItem
    {
        get => _currentItem;
        set => SetProperty(ref _currentItem, value);
    }

    private int _currentIndex;
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (SetProperty(ref _currentIndex, value) && Items.Count > 0)
                CurrentItem = Items.ElementAtOrDefault(value);
        }
    }

    private string? _nextCursor;
    private bool _hasMore = true;

    public ICommand LoadCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand ReactCommand { get; }
    public ICommand OpenDetailCommand { get; }
    public ICommand CreateMomeantCommand { get; }

    public FeedViewModel(IApiClient api, IHapticsService haptics, INavigationService nav)
    {
        _api = api;
        _haptics = haptics;
        _nav = nav;

        LoadCommand = new Command(async () => await LoadFeedAsync());
        NextCommand = new Command(async () => await NavigateNext());
        PreviousCommand = new Command(NavigatePrevious);
        ReactCommand = new Command<string>(async type => await ReactAsync(type));
        OpenDetailCommand = new Command(async () =>
        {
            if (CurrentItem is not null)
                await _nav.NavigateToAsync("MomeantDetailPage", new Dictionary<string, object> { ["momeantId"] = CurrentItem.Id });
        });
        CreateMomeantCommand = new Command(async () => await _nav.NavigateToAsync("CreateMomeantPage"));
    }

    public async Task InitializeAsync()
    {
        Items.Clear();
        _nextCursor = null;
        _hasMore = true;
        await LoadFeedAsync();
    }

    private async Task LoadFeedAsync()
    {
        if (!_hasMore || IsBusy) return;
        IsBusy = true;
        try
        {
            var url = _nextCursor is not null ? $"api/feed?cursor={Uri.EscapeDataString(_nextCursor)}&limit=10" : "api/feed?limit=10";
            var resp = await _api.GetAsync<FeedResponse>(url);
            if (resp is null) { SetError("Could not load feed."); return; }

            foreach (var item in resp.Items) Items.Add(item);
            _nextCursor = resp.NextCursor;
            _hasMore = resp.NextCursor is not null;

            if (Items.Count > 0 && CurrentItem is null)
                CurrentItem = Items[0];
        }
        finally { IsBusy = false; }
    }

    private async Task NavigateNext()
    {
        _haptics.Light();
        if (_currentIndex < Items.Count - 1)
            CurrentIndex++;
        else if (_hasMore)
            await LoadFeedAsync();
    }

    private void NavigatePrevious()
    {
        _haptics.Light();
        if (_currentIndex > 0) CurrentIndex--;
    }

    private async Task ReactAsync(string type)
    {
        if (CurrentItem is null) return;
        _haptics.Medium();
        await _api.PostAsync<object>($"api/momeants/{CurrentItem.Id}/reactions", new ReactionRequest(type));
    }
}
