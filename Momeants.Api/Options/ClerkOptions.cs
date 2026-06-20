namespace Momeants.Api.Options;

public class ClerkOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.clerk.com/v1/";
}
