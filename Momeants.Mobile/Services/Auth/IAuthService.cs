using Momeants.Shared.Contracts;

namespace Momeants.Mobile.Services.Auth;

public interface IAuthService
{
    Task<bool> StartAsync(string identifier, CancellationToken ct = default);
    Task<AuthResponse?> VerifyAsync(string identifier, string code, CancellationToken ct = default);
    Task<bool> TryRefreshAsync(CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<bool> IsAuthenticatedAsync(CancellationToken ct = default);
}
