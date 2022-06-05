using Azure.Core;
using Azure.Identity;
using BasicGraphApp.ConsoleApp.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace BasicGraphApp.ConsoleApp.Helpers;

internal class GraphHelper
{
    // Settings object
    private static Settings? _settings;
    // User auth token credential
    private static DeviceCodeCredential? _deviceCodeCredential;

    private static ClientSecretCredential? clientSecretCredential;

    // Client configured with user authentication
    private static GraphServiceClient? _userClient;

    const string TOKEN_CACHE_NAME = "MyTokenCache";

    private static DeviceCodeCredentialOptions options = new DeviceCodeCredentialOptions();

    public static async Task InitializeGraphForUserAuthAsync(Settings settings,
        Func<DeviceCodeInfo, CancellationToken, Task> deviceCodePrompt)
    {
        //DeviceCodeCredentialOptions options = new DeviceCodeCredentialOptions
        //{
        //    TokenCachePersistenceOptions = new TokenCachePersistenceOptions 
        //    { 
        //        Name = TOKEN_CACHE_NAME 
        //    },

        //};

        _settings = settings;

        _deviceCodeCredential = new DeviceCodeCredential(deviceCodePrompt,
            settings.AuthTenant, settings.ClientId, options);

        //TokenRequestContext tokenRequestContext = new TokenRequestContext(settings.GraphUserScopes);
        //var tok = _deviceCodeCredential.GetToken(tokenRequestContext);

        // Call AuthenticateAsync to fetch a new AuthenticationRecord.
        //var authRecord = await _deviceCodeCredential.AuthenticateAsync();

        //// Serialize the AuthenticationRecord to disk so that it can be re-used across executions of this initialization code.
        //using var authRecordStream = new FileStream("./DeviceAuthRecord.XXX", FileMode.OpenOrCreate, FileAccess.Write);
        //await authRecord.SerializeAsync(authRecordStream);

        _userClient = new GraphServiceClient(_deviceCodeCredential, settings.GraphUserScopes);
    }
    public static void InitializeGraphForUserAuthUsingInteractive(Settings settings)
    {
        _settings = settings;

        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = settings.AuthTenant,
            ClientId = settings.ClientId,
            //AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            // MUST be http://localhost or http://localhost:PORT
            // See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core
            RedirectUri = new Uri("http://localhost:4048"),
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = TOKEN_CACHE_NAME,
                UnsafeAllowUnencryptedStorage = true
            }
        };

        var interactiveCredential = new InteractiveBrowserCredential(options);

        // The problem is that when we use Authenticate, we cannot use our personal account (has to be work or school accounts)
        // var authRecord = interactiveCredential.Authenticate();
        //Username = authResult.Account.Username;
        //Authority = authResult.Account.Environment;
        //AccountId = authResult.Account.HomeAccountId;
        //TenantId = authResult.TenantId;
        //ClientId = clientId;



        // GetToken works but requires us to manage the token instead; lets try and avoid that
        TokenRequestContext tokenRequestContext = new TokenRequestContext(settings.GraphUserScopes);
        AccessToken tok = interactiveCredential.GetToken(tokenRequestContext);
        


        _userClient = new GraphServiceClient(interactiveCredential, settings.GraphUserScopes);


    }

    [Obsolete]
    public static void InitializeGraphForUserAuthUsingAuthorizationCode(Settings settings)
    {
        _settings = settings;

        clientSecretCredential = new ClientSecretCredential(
            settings.AuthTenant, settings.ClientId, settings.ClientSecret);

        _userClient = new GraphServiceClient(clientSecretCredential, settings.GraphUserScopes);
    }

    [Obsolete]
    public static void InitializeGraphForUserAuthUsingClientSecret(Settings settings)
    {
        _settings = settings;

        clientSecretCredential = new ClientSecretCredential(
            settings.AuthTenant, settings.ClientId, settings.ClientSecret);

        _userClient = new GraphServiceClient(clientSecretCredential, settings.GraphUserScopes);
    }

    [Obsolete]
    public static void InitializeGraphForUserAuthUsingObo(Settings settings)
    {
        _settings = settings;

        // This is the incoming token to exchange using on-behalf-of flow
        var oboToken = "JWT_TOKEN_TO_EXCHANGE";

        var cca = ConfidentialClientApplicationBuilder
            .Create(settings.ClientId)
            .WithTenantId(settings.AuthTenant)
            .WithClientSecret(settings.ClientSecret)
            .Build();

        var authProvider = new DelegateAuthenticationProvider(async (request) => {
            // Use Microsoft.Identity.Client to retrieve token
            var assertion = new UserAssertion(oboToken);
            var result = await cca.AcquireTokenOnBehalfOf(settings.GraphUserScopes, assertion).ExecuteAsync();

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
        });

        //var graphClient = new GraphServiceClient(authProvider);

        //_userClient = new GraphServiceClient(clientSecretCredential, settings.GraphUserScopes);
        _userClient = new GraphServiceClient(authProvider);
    }

    public static async Task<string> GetUserTokenAsync()
    {
        // Ensure credential isn't null
        _ = _deviceCodeCredential ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        // Ensure scopes isn't null
        _ = _settings?.GraphUserScopes ?? throw new System.ArgumentNullException("Argument 'scopes' cannot be null");

        // Request token with given scopes
        var context = new TokenRequestContext(_settings.GraphUserScopes);
        var response = await _deviceCodeCredential.GetTokenAsync(context);
        return response.Token;
    }

    public static Task<User> GetUserAsync()
    {
        // Ensure client isn't null
        _ = _userClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        return _userClient.Me
            .Request()
            .Select(u => new
            {
            // Only request specific properties
                u.DisplayName,
                u.Mail,
                u.UserPrincipalName
            })
            .GetAsync();
    }

    public static Task<IMailFolderMessagesCollectionPage> GetInboxAsync()
    {
        // Ensure client isn't null
        _ = _userClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        return _userClient.Me
            // Only messages from Inbox folder
            .MailFolders["Inbox"]
            .Messages
            .Request()
            .Select(m => new
            {
            // Only request specific properties
                m.From,
                m.IsRead,
                m.ReceivedDateTime,
                m.Subject
            })
            // Get at most 25 results
            .Top(25)
            // Sort by received time, newest first
            .OrderBy("ReceivedDateTime DESC")
            .GetAsync();
    }

    public static Task<ITodoListsCollectionPage> GetTaskAsync()
    {
        // Ensure client isn't null
        _ = _userClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        return _userClient.Me
            .Todo
            .Lists
            .Request()
            .GetAsync();
    }

    public static async Task SendMailAsync(string subject, string body, string recipient)
    {
        // Ensure client isn't null
        _ = _userClient ??
            throw new System.NullReferenceException("Graph has not been initialized for user auth");

        // Create a new message
        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                Content = body,
                ContentType = BodyType.Text
            },
            ToRecipients = new Recipient[]
            {
            new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = recipient
                }
            }
            }
        };

        // Send the message
        await _userClient.Me
            .SendMail(message)
            .Request()
            .PostAsync();
    }


}
