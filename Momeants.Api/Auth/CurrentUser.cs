namespace Momeants.Api.Auth;

public class CurrentUser
{
    public Guid UserId { get; set; }
    public string ClerkUserId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? DeviceId { get; set; }
    public string AccountStatus { get; set; } = "active";
}
