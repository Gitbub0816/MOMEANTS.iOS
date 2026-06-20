using Momeants.Mobile.Services.Auth;
using Momeants.Mobile.Services.Storage;
using Momeants.Shared.Contracts;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Momeants.Mobile.Services.Api;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly ISecureTokenStore _tokenStore;
    private readonly IAuthService _authService;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(HttpClient http, ISecureTokenStore tokenStore, IAuthService authService)
    {
        _http = http;
        _tokenStore = tokenStore;
        _authService = authService;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, path);
        return await SendAsync<T>(req, ct);
    }

    public async Task<T?> PostAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, path);
        if (body is not null) req.Content = JsonContent.Create(body);
        return await SendAsync<T>(req, ct);
    }

    public async Task<T?> PatchAsync<T>(string path, object? body = null, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, path);
        if (body is not null) req.Content = JsonContent.Create(body);
        return await SendAsync<T>(req, ct);
    }

    public async Task<bool> DeleteAsync(string path, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, path);
        var resp = await SendWithAuthAsync(req, ct);
        return resp?.IsSuccessStatusCode == true;
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage req, CancellationToken ct)
    {
        var resp = await SendWithAuthAsync(req, ct);
        if (resp is null || !resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
    }

    private async Task<HttpResponseMessage?> SendWithAuthAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        if (token is not null)
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _http.SendAsync(req, ct);

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Try refresh
            var refreshed = await _authService.TryRefreshAsync(ct);
            if (refreshed)
            {
                var newToken = await _tokenStore.GetAccessTokenAsync();
                var retry = CloneRequest(req);
                if (newToken is not null)
                    retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                resp = await _http.SendAsync(retry, ct);
            }
        }

        return resp;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        foreach (var h in original.Headers) clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
        // Note: Content is not cloned (body can't be re-sent after disposal); callers should avoid retry on POST with body
        return clone;
    }
}
