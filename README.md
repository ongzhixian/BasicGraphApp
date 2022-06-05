# BasicGraphApp

A basic .NET Core Microsoft Graph console application.


```ps1: In C:\src\github.com\ongzhixian\BasicGraphApp

dotnet new sln -n BasicGraphApp
dotnet new console -n BasicGraphApp.ConsoleApp
dotnet sln .\BasicGraphApp.sln add .\BasicGraphApp.ConsoleApp\

dotnet add .\BasicGraphApp.ConsoleApp\ package Microsoft.Extensions.Configuration
dotnet add .\BasicGraphApp.ConsoleApp\ package Microsoft.Extensions.Configuration.Binder
dotnet add .\BasicGraphApp.ConsoleApp\ package Microsoft.Extensions.Configuration.Json
dotnet add .\BasicGraphApp.ConsoleApp\ package Microsoft.Extensions.Configuration.UserSecrets

dotnet add .\BasicGraphApp.ConsoleApp\ package Azure.Identity
dotnet add .\BasicGraphApp.ConsoleApp\ package Microsoft.Graph

dotnet user-secrets --project .\BasicGraphApp.ConsoleApp\ init
dotnet user-secrets --project .\BasicGraphApp.ConsoleApp\ set "azureApps:basicGraphApp:tenantId" "<tenantId-value:guid?>"
dotnet user-secrets --project .\BasicGraphApp.ConsoleApp\ set "azureApps:basicGraphApp:clientSecret" "<clientSecret-value>"

```

## Optional secrets

The following secrets are only applicable to the App-Only Azure Authentication flows 
1.  `azureApps:basicGraphApp:tenantId`
2.  `azureApps:basicGraphApp:clientSecret`
See: https://docs.microsoft.com/en-us/graph/sdks/choose-authentication-providers
Note:   App-Only -- also know as daemon uses the client credentials provider.
        This authentication method cannot do user-account delegation.


Other ways to extend configuration
```

Microsoft.Extensions.Configuration.CommandLine 
Microsoft.Extensions.Configuration.Binder 
Microsoft.Extensions.Configuration.EnvironmentVariables

```
