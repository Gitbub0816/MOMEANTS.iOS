namespace Momeants.Mobile.Services.Api;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken ct = default);
    Task<T?> PostAsync<T>(string path, object? body = null, CancellationToken ct = default);
    Task<T?> PatchAsync<T>(string path, object? body = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(string path, CancellationToken ct = default);
}
