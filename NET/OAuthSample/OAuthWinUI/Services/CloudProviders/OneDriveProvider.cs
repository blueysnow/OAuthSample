using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using OAuthShared;
using OAuthWinUI.Helpers;

using Windows.Storage;

namespace OAuthWinUI.Services.CloudProviders;
internal partial class OneDriveProvider : ObservableObject, ICloudProvider, IAuthenticationProvider
{
    #region OAuthSettings

    protected static class OAuthenticationSettings
    {
        public const string ClientId = "15afedda-afd1-4b8b-a572-56df47e1211e"; // < insert-client-id-here>";
        public const string TenantId = "common";
        public const string RedirectUri = "http://localhost";

        public static readonly string[] Scopes =
        [
            "https://graph.microsoft.com/.default"
        ];
    }

    #endregion

    #region Private Field(s)

    private readonly GraphServiceClient client;
    private string userIdentifier = string.Empty;
    private readonly Lazy<Task<IPublicClientApplication>> publicClientApplication;
    private readonly CreateUploadSessionPostRequestBody uploadSessionRequestBody;

    #endregion

    #region Constructor(s)

    public OneDriveProvider()
    {
        uploadSessionRequestBody = new CreateUploadSessionPostRequestBody
        {
            Item = new DriveItemUploadableProperties
            {
                AdditionalData = new Dictionary<string, object>
                {
                    {
                        "@microsoft.graph.conflictBehavior", "replace"
                    }
                }
            }
        };

        client = new GraphServiceClient(this);
        publicClientApplication = new Lazy<Task<IPublicClientApplication>>(InitializeMsalWithCache);
    }

    #endregion

    #region IAuthenticationProvider Implementation(s)

    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        if (request.URI.Host == "graph.microsoft.com")
        {
            // First try to get the token silently
            AuthenticationResult? result = await GetTokenSilentlyAsync();

            // If silent acquisition fails, try interactive
            result ??= await GetTokenInteractivelyAsync();

            request.Headers.Add("Authorization", $"Bearer {result?.AccessToken}");
        }
    }

    #endregion

    #region Private Method(s)

    /// <summary>
    ///     Attempt to acquire a token silently (no prompts).
    /// </summary>
    private async Task<AuthenticationResult?> GetTokenSilentlyAsync()
    {
        try
        {
            IPublicClientApplication pca = await publicClientApplication.Value;

            IAccount? account = await GetUserAccountAsync();
            return account == null
                ? null
                : await pca.AcquireTokenSilent(OAuthenticationSettings.Scopes, account)
                    .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Get the user account from the MSAL cache.
    /// </summary>
    private async Task<IAccount?> GetUserAccountAsync()
    {
        try
        {
            IPublicClientApplication pca = await publicClientApplication.Value;

            if (string.IsNullOrEmpty(userIdentifier))
            {
                // If no saved user ID, then get the first account.
                // There should only be one account in the cache anyway.
                IEnumerable<IAccount> accounts = await pca.GetAccountsAsync();
                IAccount? account = accounts.FirstOrDefault();

                // Save the user ID so this is easier next time
                userIdentifier = account?.HomeAccountId.Identifier ?? string.Empty;
                return account;
            }

            // If there's a saved user ID use it to get the account
            return await pca.GetAccountAsync(userIdentifier);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Initializes a PublicClientApplication with a secure serialized cache.
    /// </summary>
    private async Task<IPublicClientApplication> InitializeMsalWithCache()
    {
        // Initialize the PublicClientApplication
        PublicClientApplicationBuilder builder = PublicClientApplicationBuilder
            .Create(OAuthenticationSettings.ClientId)
            .WithRedirectUri(OAuthenticationSettings.RedirectUri);

        IPublicClientApplication pca = builder.Build();

        await RegisterMsalCacheAsync(pca.UserTokenCache);

        return pca;
    }

    /// <summary>
    ///     Attempts to get a token interactively using the device's browser.
    /// </summary>
    private async Task<AuthenticationResult?> GetTokenInteractivelyAsync()
    {
        try
        {
            IPublicClientApplication pca = await publicClientApplication.Value;

            AuthenticationResult result = await pca.AcquireTokenInteractive(OAuthenticationSettings.Scopes)
                .ExecuteAsync();

            // Store the user ID to make account retrieval easier
            userIdentifier = result.Account.HomeAccountId.Identifier;
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

    }

    private async Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        var directoryPath = RuntimeHelper.IsMSIX ? ApplicationData.Current.LocalCacheFolder.Path : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OAuthWinUI");
        // Configure storage properties for cross-platform
        // See https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache
        StorageCreationProperties storageProperties = new StorageCreationPropertiesBuilder(
                "msal.oauthsample.cache",
                directoryPath)
            .Build();

        // Create a cache helper
        MsalCacheHelper cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);

        // Connect the PublicClientApplication's cache with the cacheHelper.
        // This will cause the cache to persist into secure storage on the device.
        cacheHelper.RegisterCache(tokenCache);
    }

    #endregion

    #region ICloudProvider Implementation(s)

    public async Task<bool> SignInAsync()
    {
        try
        {
            // First attempt to get a token silently
            AuthenticationResult? result = await GetTokenSilentlyAsync();

            // If silent attempt didn't work, try an
            // interactive sign in
            result ??= await GetTokenInteractivelyAsync();

            return result is not null; ;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> SignOutAsync()
    {
        IPublicClientApplication pca = await publicClientApplication.Value;

        // Get all accounts (there should only be one)
        // and remove them from the cache
        IEnumerable<IAccount> accounts = await pca.GetAccountsAsync();
        foreach (IAccount? account in accounts)
        {
            await pca.RemoveAsync(account);
        }

        // Clear the user identifier
        userIdentifier = string.Empty;

        return true;
    }

    public async Task<UserDetails> GetUserDetailsAsync()
    {
        User? user = await client.Me.GetAsync();
        return user is not null ? new UserDetails(user.UserPrincipalName ?? string.Empty, user.DisplayName ?? string.Empty, user.GivenName ?? string.Empty, user.Surname ?? string.Empty, user.Mail ?? string.Empty) : new UserDetails();
    }

    #endregion

}