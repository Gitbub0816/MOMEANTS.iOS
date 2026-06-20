using Momeants.Mobile.Services.Storage;
using Momeants.Shared.Contracts;
using System.Net.Http.Json;

namespace Momeants.Mobile.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ISecureTokenStore _tokenStore;

    public AuthService(HttpClient http, ISecureTokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    public async Task<bool> StartAsync(string identifier, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/start", new StartAuthRequest(identifier), ct);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<AuthResponse?> VerifyAsync(string identifier, string code, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/auth/verify", new VerifyAuthRequest(identifier, code), ct);
            if (!resp.IsSuccessStatusCode) return null;
            var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct);
            if (auth is not null)
            {
                await _tokenStore.SaveAccessTokenAsync(auth.AccessToken);
                await _tokenStore.SaveRefreshTokenAsync(auth.RefreshToken);
            }
            return auth;
        }
        catch { return null; }
    }

    public async Task<bool> TryRefreshAsync(CancellationToken ct = default)
    {
        try
        {
            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var resp = await _http.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest(refreshToken), ct);
            if (!resp.IsSuccessStatusCode) return false;

            var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct);
            if (auth is not null)
            {
                await _tokenStore.SaveAccessTokenAsync(auth.AccessToken);
                await _tokenStore.SaveRefreshTokenAsync(auth.RefreshToken);
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
                await _http.PostAsJsonAsync("api/auth/logout", new RefreshTokenRequest(refreshToken), ct);
        }
        catch { }
        finally { await _tokenStore.ClearAllAsync(); }
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default)
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
