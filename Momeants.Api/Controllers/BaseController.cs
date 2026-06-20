using Microsoft.AspNetCore.Mvc;
using Momeants.Api.Auth;
using Momeants.Shared.Contracts;

namespace Momeants.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected CurrentUser? GetCurrentUser() => HttpContext.Items["CurrentUser"] as CurrentUser;

    protected IActionResult RequireAuth(out CurrentUser user)
    {
        var u = GetCurrentUser();
        if (u is null)
        {
            user = null!;
            return Unauthorized(ErrorEnvelope.From("unauthorized", "Authentication required."));
        }
        if (u.AccountStatus == "suspended")
        {
            user = null!;
            return StatusCode(403, ErrorEnvelope.From("account_suspended", "Your account has been suspended."));
        }
        user = u;
        return null!;
    }

    protected bool TryGetCurrentUser(out CurrentUser user)
    {
        var u = GetCurrentUser();
        user = u!;
        return u is not null;
    }

    protected IActionResult ApiError(string code, string message, int statusCode = 400)
        => StatusCode(statusCode, ErrorEnvelope.From(code, message));
}
