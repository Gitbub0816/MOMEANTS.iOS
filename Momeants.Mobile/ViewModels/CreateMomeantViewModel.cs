using Momeants.Mobile.Services.Api;
using Momeants.Mobile.Services.Media;
using Momeants.Mobile.Services.Navigation;
using Momeants.Shared.Contracts;
using System.Windows.Input;

namespace Momeants.Mobile.ViewModels;

public class CreateMomeantViewModel : BaseViewModel
{
    private readonly IApiClient _api;
    private readonly IMediaUploadService _mediaUpload;
    private readonly INavigationService _nav;

    private string? _caption;
    public string? Caption { get => _caption; set => SetProperty(ref _caption, value); }

    private string _audienceType = "friends";
    public string AudienceType { get => _audienceType; set => SetProperty(ref _audienceType, value); }

    private ImageSource? _selectedImage;
    public ImageSource? SelectedImage { get => _selectedImage; set => SetProperty(ref _selectedImage, value); }

    private Stream? _selectedStream;
    private string _selectedMimeType = "image/jpeg";
    private string _selectedExtension = "jpg";

    public ICommand PickPhotoCommand { get; }
    public ICommand PostCommand { get; }
    public ICommand CancelCommand { get; }

    public CreateMomeantViewModel(IApiClient api, IMediaUploadService mediaUpload, INavigationService nav)
    {
        _api = api;
        _mediaUpload = mediaUpload;
        _nav = nav;

        PickPhotoCommand = new Command(async () => await PickPhotoAsync());
        PostCommand = new Command(async () => await PostAsync(), () => !IsBusy && _selectedStream is not null);
        CancelCommand = new Command(async () => await _nav.GoBackAsync());
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            var results = await MediaPicker.Default.PickPhotosAsync();
            var result = results?.FirstOrDefault();
            if (result is null) return;

            _selectedStream = await result.OpenReadAsync();
            _selectedMimeType = result.ContentType ?? "image/jpeg";
            _selectedExtension = Path.GetExtension(result.FileName).TrimStart('.') is { Length: > 0 } ext ? ext : "jpg";
            SelectedImage = ImageSource.FromStream(() => _selectedStream);
        }
        catch (Exception ex)
        {
            SetError($"Could not pick photo: {ex.Message}");
        }
    }

    private async Task PostAsync()
    {
        if (_selectedStream is null) { SetError("Please select a photo."); return; }
        ClearError();
        IsBusy = true;
        try
        {
            _selectedStream.Position = 0;
            var uploadResult = await _mediaUpload.UploadImageAsync(_selectedStream, _selectedMimeType, _selectedExtension);
            if (!uploadResult.Success || uploadResult.MediaId is null)
            {
                SetError(uploadResult.Error ?? "Upload failed.");
                return;
            }

            var createReq = new CreateMomeantRequest(uploadResult.MediaId.Value, Caption, null, AudienceType);
            var momeant = await _api.PostAsync<MomeantDto>("api/momeants", createReq);
            if (momeant is null) { SetError("Failed to create momeant."); return; }

            await _nav.NavigateToRootAsync("FeedPage");
        }
        finally { IsBusy = false; }
    }
}
