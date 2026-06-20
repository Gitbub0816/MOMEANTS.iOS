namespace Momeants.Mobile.Services.Storage;

public interface ISecureTokenStore
{
    Task SaveAccessTokenAsync(string token);
    Task SaveRefreshTokenAsync(string token);
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task ClearAllAsync();
}
