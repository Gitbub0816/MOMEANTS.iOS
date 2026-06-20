namespace Momeants.Mobile.Services.Storage;

public class SecureTokenStore : ISecureTokenStore
{
    private const string AccessTokenKey = "momeants_access_token";
    private const string RefreshTokenKey = "momeants_refresh_token";

    public async Task SaveAccessTokenAsync(string token)
        => await SecureStorage.Default.SetAsync(AccessTokenKey, token);

    public async Task SaveRefreshTokenAsync(string token)
        => await SecureStorage.Default.SetAsync(RefreshTokenKey, token);

    public Task<string?> GetAccessTokenAsync()
        => SecureStorage.Default.GetAsync(AccessTokenKey);

    public Task<string?> GetRefreshTokenAsync()
        => SecureStorage.Default.GetAsync(RefreshTokenKey);

    public Task ClearAllAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        return Task.CompletedTask;
    }
}
