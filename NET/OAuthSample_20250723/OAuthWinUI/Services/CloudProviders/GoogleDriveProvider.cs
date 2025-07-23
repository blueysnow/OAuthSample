using System.Diagnostics;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using OAuthShared;

namespace OAuthWinUI.Services.CloudProviders;

internal class GoogleDriveProvider : ICloudProvider
{
    #region GDAuthSettings

    private static class GdAuthenticationSettings
    {
        public const string ClientId = "<insert-client-id-here>";
        public const string ClientSecret = "<insert-client-secret-here>";
        public const string ApplicationName = "OAuthWinUI";

        public const string AccessTokenKey = "AccessToken";
        public const string RefreshTokenKey = "RefreshToken";
        public const string IdTokenKey = "IdToken";
        public const string IssueDateKey = "IssueDate";
        public const string ExpiresInSecondsKey = "Expires";
    }

    #endregion

    #region Private Fields

    private Oauth2Service? profileService;
    private UserCredential? credential;

    private readonly string[] scopes =
    {
        Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail
    };

    #endregion

    #region Private Methods

    private async Task<UserCredential?> CreateGoogleDriveCredential()
    {
        try
        {
            UserCredential? userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = GdAuthenticationSettings.ClientId,
                    ClientSecret = GdAuthenticationSettings.ClientSecret
                },
                scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(GdAuthenticationSettings.ApplicationName)
            );

            return userCredential;
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
            await GoogleWebAuthorizationBroker.ReauthorizeAsync(credential, CancellationToken.None);
        }
        else
        {
            credential = await CreateGoogleDriveCredential();
        }
    }

    private async Task<Oauth2Service> CreateOauthService()
    {
        if (credential is null) { await SignInAsync(); }
        Oauth2Service service = new(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = GdAuthenticationSettings.ApplicationName
        });

        return service;
    }

    #endregion

    #region ICloudProvider Implementation(s)

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

            Userinfo user = await profileService.Userinfo.Get().ExecuteAsync();

            return user is not null ? new UserDetails(user.Email, user.Name, user.GivenName, user.FamilyName, user.Email) : new();
        }
        catch (TokenResponseException exception)
        {
            Debug.WriteLine(exception.Error);
            await ReauthorizeTokenAsync();
            profileService = await CreateOauthService();

            if (profileService == null)
            {
                return new();
            }
            Userinfo user = await profileService.Userinfo.Get().ExecuteAsync();
            return user is not null ? new UserDetails(user.Email, user.Name, user.GivenName, user.FamilyName, user.Email) : new();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception.Message);
            throw;
        }

    }

    #endregion

}