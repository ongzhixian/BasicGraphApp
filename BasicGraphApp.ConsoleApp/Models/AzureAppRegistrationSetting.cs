namespace BasicGraphApp.ConsoleApp.Models;

public class AzureAppRegistrationSetting
{
    public string AuthenticationTenant { get; set; }

    public string ClientId { get; set; }

    public string? TenantId { get; set; }
    
    public string? ClientSecret { get; set; }

    public string[]? MicrosoftGraphScopes { get; set; }
}