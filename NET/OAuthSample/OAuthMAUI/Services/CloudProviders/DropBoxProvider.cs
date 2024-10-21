
using Dropbox.Api;
using CommunityToolkit.Mvvm.ComponentModel;
using Dropbox.Api.Users;

using OAuthShared;
using System.Diagnostics;
using System.Net;

using Faithlife.Utility;
using Newtonsoft.Json;

namespace OAuthMAUI.Services.CloudProviders;
internal partial class DropBoxProvider : ObservableRecipient, ICloudProvider
{
    #region Constructor

    public DropBoxProvider()
    {
        pKceoAuthFlow = new PKCEOAuthFlow();
        AuthenticationUrl = GenerateAuthenticationUrl();
        AuthenticationResponse = default;
        Client = default;
        CurrentUser = new UserDetails();
    }

    #endregion

    #region OAuthSettings

    private static class DAuthenticationSettings
    {
        public const string AppKey = "<insert-app-key-here>";
        public const string AppSecret = "<insert-app-secret-here>";
        public const string RedirectUri = "https://eft-upward-bison.ngrok-free.app/oauth/dropbox/signin";
        public const string DesktopRedirectUri = "oauthmaui://";
    }

    #endregion

    #region Private Fields

    [ObservableProperty]
    private UserDetails currentUser;

    private PKCEOAuthFlow pKceoAuthFlow;

    #endregion

    #region Properties

    private string AuthenticationUrl
    {
        get; set;
    }
    private string? OAuthToState
    {
        get; set;
    }

    private OAuth2Response? AuthenticationResponse
    {
        get; set;
    }

    private DropboxClient? Client
    {
        get; set;
    }

    #endregion

    #region Private Methods

    private async Task<OAuth2Response?> AuthorizeDropbox()
    {
        try
        {
            if (AuthenticationResponse is not null && string.IsNullOrWhiteSpace(AuthenticationResponse.AccessToken) == false)
            {
                // Already authorized
                //OnAuthenticated?.Invoke();
                return AuthenticationResponse;
            }

            AuthenticationUrl = GenerateAuthenticationUrl();
            UriBuilder dropBoxRedirectUri = new(DAuthenticationSettings.RedirectUri);

            // encode the authentication url and pass it as a query to the web api url
            var encodedAuthUrl = WebUtility.UrlEncode(AuthenticationUrl);
            var webApiUrl = $"https://eft-upward-bison.ngrok-free.app/oauth/dropbox?auth_uri={encodedAuthUrl}";

#if WINDOWS
            var response = await WinUIEx.WebAuthenticator.AuthenticateAsync(new Uri(AuthenticationUrl), new Uri(DAuthenticationSettings.DesktopRedirectUri), new CancellationTokenSource().Token);
#else
            var response = await WebAuthenticator.AuthenticateAsync(new Uri(webApiUrl), new Uri(DAuthenticationSettings.DesktopRedirectUri));
#endif

            var responseProps = response.Properties;
            var hasCodeKey = responseProps.TryGetValue("code", out var codeValue);
            var hasStateKey = responseProps.TryGetValue("state", out var stateValue);
            if (!hasCodeKey || !hasStateKey)
            {
                return AuthenticationResponse;
            }
            _ = dropBoxRedirectUri.AppendQuery($"code={codeValue}");
            _ = dropBoxRedirectUri.AppendQuery($"state={stateValue}");

           AuthenticationResponse = await pKceoAuthFlow.ProcessCodeFlowAsync(dropBoxRedirectUri.Uri, DAuthenticationSettings.AppKey, DAuthenticationSettings.RedirectUri, OAuthToState);
            return AuthenticationResponse;
        }
        catch (TaskCanceledException)
        {
            Dictionary<string, string> response = new()
            {
                {
                    "error", "task_canceled"
                },
                {
                    "error_description", "The task was canceled by the user"
                }
            };

            Debug.WriteLine("DropBox Authentication Error " + response);
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private DropboxClient CreateDropboxClient()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(AuthenticationResponse);
            ArgumentException.ThrowIfNullOrWhiteSpace(AuthenticationResponse.AccessToken);
            var expiresAt = AuthenticationResponse.ExpiresAt ?? DateTime.Now;

#if __ANDROID__
			return new DropboxClient(AuthenticationResponse.AccessToken, AuthenticationResponse.RefreshToken, expiresAt, DAuthenticationSettings.AppKey, DAuthenticationSettings.AppSecret, new DropboxClientConfig()
			{
				HttpClient = new HttpClient(new SocketsHttpHandler())
			});
#else
             return new DropboxClient(AuthenticationResponse.AccessToken, AuthenticationResponse.RefreshToken, expiresAt, DAuthenticationSettings.AppKey, DAuthenticationSettings.AppSecret);
#endif
        }

        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }

    private string GenerateAuthenticationUrl()
    {
        try
        {
            OAuthToState = Guid.NewGuid().ToString("N");
            Uri authorizeUri = pKceoAuthFlow.GetAuthorizeUri(OAuthResponseType.Code, DAuthenticationSettings.AppKey, DAuthenticationSettings.RedirectUri, OAuthToState, tokenAccessType: TokenAccessType.Offline);
            return authorizeUri.AbsoluteUri;
        }
        catch (Exception exception)
        {           

            Console.WriteLine(@"Obtaining DropBox Authentication URL Error " + exception.Message);
            throw;
        }
    }

    #endregion

    #region ICloudProvider Implementation

    public async Task<bool> SignInAsync()
    {
        try
        {
            if (AuthenticationResponse is not null && !string.IsNullOrWhiteSpace(AuthenticationResponse.AccessToken))
            {
                Client = CreateDropboxClient();
            }
            else
            {
                if (string.IsNullOrEmpty(AuthenticationUrl))
                {
                    throw new Exception("AuthenticationURL is not generated !");
                }
                AuthenticationResponse = await AuthorizeDropbox();
                Client = CreateDropboxClient();
            }

            if (Client is null)
            {
                Debug.WriteLine("DropBox Client is null");
                return false;
            }

            // Get Current user details
            var dropboxUser = await Client.Users.GetCurrentAccountAsync();
            CurrentUser = new UserDetails(dropboxUser.Name.DisplayName, dropboxUser.Name.DisplayName, dropboxUser.Name.GivenName, dropboxUser.Name.Surname, dropboxUser.Email);

            return true;
        }
        catch (Exception exception)
        {
            Debug.WriteLine("DropBox Authentication Error " + exception.Message);
            return false;
        }
    }

    public async Task<bool> SignOutAsync()
    {
        try
        {
            if (Client is not null)
            {
                await Client.Auth.TokenRevokeAsync();
            }
            AuthenticationResponse = null;
            Client = null;
            CurrentUser = new UserDetails();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task<UserDetails> GetUserDetailsAsync()
    {
        try
        {
            return Task.FromResult(CurrentUser);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    #endregion
}