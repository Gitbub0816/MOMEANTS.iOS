using Microsoft.EntityFrameworkCore;
using Momeants.Api.Auth;
using Momeants.Api.Data;
using Momeants.Api.Services;

namespace Momeants.Api.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService tokenService, AppDbContext db)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader["Bearer ".Length..];
            if (tokenService.ValidateAccessToken(token, out var userId, out var clerkUserId))
            {
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

                if (user is not null)
                {
                    var currentUser = new CurrentUser
                    {
                        UserId = user.Id,
                        ClerkUserId = user.ClerkUserId,
                        AccountStatus = user.AccountStatus,
                        DeviceId = context.Request.Headers["X-Device-Id"].FirstOrDefault()
                    };
                    context.Items["CurrentUser"] = currentUser;
                }
            }
        }

        await _next(context);
    }
}
