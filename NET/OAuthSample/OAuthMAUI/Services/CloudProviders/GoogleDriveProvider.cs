using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using OAuthMAUI.Services.CloudProviders.Helpers;
using Google.Apis.Oauth2.v2;
using OAuthShared;
using Google.Apis.Oauth2.v2.Data;
namespace OAuthMAUI.Services.CloudProviders;
internal class GoogleDriveProvider : ICloudProvider
{
    #region GDAuthSettings

    private static class GdAuthenticationSettings
    {
        public const string DesktopClientId = "<insert-desktop-client-id-here>";
        public const string DesktopClientSecret = "<insert-desktop-client-secret-here>";
        public const string DesktopApplicationName = "OAuthWinUI";

        public const string WebApplicationClientId = "<insert-web-app-client-id-here>";
        public const string WebApplicationClientSecret = "<insert-web-app-client-secret-here";
        public const string WebApplicationName = "OAuthMAUI";
    }

    #endregion

    #region Private Fields

    private Oauth2Service? profileService;
    private UserCredential? credential;

    private readonly string[] scopes =
    [
       Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail
    ];

    #endregion

    #region Private Methods

    private async Task<UserCredential?> CreateGoogleDriveCredential()
    {
        try
        {
            const string clientId = GdAuthenticationSettings.WebApplicationClientId;
            const string clientSecret = GdAuthenticationSettings.WebApplicationClientSecret;
            var applicationName = DeviceInfo.Current.Idiom == DeviceIdiom.Desktop ? GdAuthenticationSettings.WebApplicationName : Path.Combine(FileSystem.Current.CacheDirectory, GdAuthenticationSettings.WebApplicationName);
            ICodeReceiver codeReceiver = new GoogleCloudCodeReceiver();

            var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(applicationName, DeviceInfo.Current.Idiom != DeviceIdiom.Desktop),
                codeReceiver);
            return credentials;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine(@"Task was cancelled");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private async Task ReauthorizeTokenAsync()
    {
        if (credential is not null)
        {
            ICodeReceiver? codeReceiver = DeviceInfo.Current.Idiom != DeviceIdiom.Desktop ? new GoogleCloudCodeReceiver() : null;
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(credential, CancellationToken.None, codeReceiver);
        }
        else
        {
            credential = await CreateGoogleDriveCredential();
        }
    }

    private async Task<Oauth2Service> CreateOauthService()
    {
        if (credential is null) { await SignInAsync(); }

#if ANDROID || IPADOS || IOS
        Oauth2Service service = new(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = GdAuthenticationSettings.WebApplicationName
        });
#else
		Oauth2Service service = new(new BaseClientService.Initializer
		{
			HttpClientInitializer = credential, ApplicationName = GdAuthenticationSettings.DesktopApplicationName
		});
#endif

        return service;
    }

    #endregion

    #region ICloudProvider Implementation

    public async Task<bool> SignInAsync()
    {
        credential ??= await CreateGoogleDriveCredential();
        return (credential is not null);
    }

    public async Task<bool> SignOutAsync() => credential == null || await credential.RevokeTokenAsync(CancellationToken.None);

    public async Task<UserDetails> GetUserDetailsAsync()
    {
        try
        {
            profileService = await CreateOauthService();

            if (profileService == null)
            {
                return new UserDetails();
            }
            Userinfo user = await profileService.Userinfo.Get().ExecuteAsync();
            return user is not null ? new UserDetails(user.Email, user.Name, user.GivenName, user.FamilyName, user.Email) : new UserDetails();
        }
        catch (TokenResponseException ex)
        {
            Console.WriteLine(ex.Error);
            await ReauthorizeTokenAsync();
            profileService = await CreateOauthService();

            if (profileService == null)
            {
                return new UserDetails();
            }
            Userinfo user = await profileService.Userinfo.Get().ExecuteAsync();
            return user is not null ? new UserDetails(user.Email, user.Name, user.GivenName, user.FamilyName, user.Email) : new UserDetails();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }

    #endregion
}
