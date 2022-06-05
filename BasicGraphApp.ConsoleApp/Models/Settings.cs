using Microsoft.Extensions.Configuration;

namespace BasicGraphApp.ConsoleApp.Models;

public class Settings
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TenantId { get; set; }
    public string? AuthTenant { get; set; }
    public string[]? GraphUserScopes { get; set; }

    public static Settings LoadSettings()
    {
        // Load settings
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>()
            .Build();

        return config.GetRequiredSection("settings").Get<Settings>();
    }
}


