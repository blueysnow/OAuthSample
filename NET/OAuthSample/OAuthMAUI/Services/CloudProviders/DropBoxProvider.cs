
using Dropbox.Api;
using CommunityToolkit.Mvvm.ComponentModel;
using Dropbox.Api.Users;

using OAuthShared;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OAuthMAUI.Services.CloudProviders;
internal partial class DropBoxProvider : ObservableRecipient, ICloudProvider
{
    #region Constructor

    public DropBoxProvider()
    {
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
        public const string RedirectUri = "<insert-redirect-uri-here>"; 
    }

    #endregion

    #region Private Fields

    private const string DropBoxTokenResponseKey = "dropbox_token_response";

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private UserDetails currentUser;

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

    private async Task AuthorizeDropbox()
    {
        try
        {
            if (AuthenticationResponse is not null && string.IsNullOrWhiteSpace(AuthenticationResponse.AccessToken) == false)
            {
                // Already authorized
                //OnAuthenticated?.Invoke();
                return;
            }

            AuthenticationUrl ??= GenerateAuthenticationUrl();

            WebView webView = new()
            {
                Source = new UrlWebViewSource
                {
                    Url = AuthenticationUrl
                }
            };
            webView.Navigating += WebViewNavigating;
            ContentPage contentPage = new()
            {
                Content = webView
            };
            await Shell.Current.Navigation.PushModalAsync(contentPage);
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private DropboxClient CreateDropboxClient()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(AuthenticationResponse);
            ArgumentException.ThrowIfNullOrWhiteSpace(AuthenticationResponse.AccessToken);
            DateTime expiresAt = AuthenticationResponse.ExpiresAt ?? DateTime.Now;

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
            Uri authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, DAuthenticationSettings.AppKey, new Uri(DAuthenticationSettings.RedirectUri), OAuthToState);
            return authorizeUri.AbsoluteUri;
        }
        catch (Exception exception)
        {           

            Console.WriteLine(@"Obtaining DropBox Authentication URL Error " + exception.Message);
            throw;
        }
    }

    private async void WebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith(DAuthenticationSettings.RedirectUri, StringComparison.OrdinalIgnoreCase))
        {
            // we need to ignore all navigation that isn't to the redirect uri.
            return;
        }

        try
        {
            OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(new Uri(e.Url));

            if (result.State != OAuthToState)
            {
                return;
            }

            AuthenticationResponse = result;

            Debug.WriteLine($"ACCESS TOKEN == {AuthenticationResponse.AccessToken}");

            Debug.WriteLine($"REFRESH TOKEN == {AuthenticationResponse.RefreshToken}");
            Debug.WriteLine($"EXPIRES AT == {AuthenticationResponse.ExpiresAt}");
            Debug.WriteLine($"TOKEN TYPE == {AuthenticationResponse.TokenType}");
            Debug.WriteLine($"Serialized Response == {JsonConvert.SerializeObject(AuthenticationResponse)})");

            Client = CreateDropboxClient();

            // Get Current user details
            FullAccount dropboxUser = await Client.Users.GetCurrentAccountAsync();
            CurrentUser = new UserDetails(dropboxUser.Name.DisplayName, dropboxUser.Name.DisplayName, dropboxUser.Name.GivenName, dropboxUser.Name.Surname, dropboxUser.Email);
        }
        catch (ArgumentException)
        {
            // There was an error in the URI passed to ParseTokenFragment
            throw;
        }
        catch (InvalidOperationException)
        {
            // There was an error processing the response
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            e.Cancel = true;
            _ = await Shell.Current.Navigation.PopModalAsync();
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
                await AuthorizeDropbox();
            }

            if (Client is null)
            {
                Debug.WriteLine("DropBox Client is null");
                return false;
            }

            // Get Current user details
            FullAccount dropboxUser = await Client.Users.GetCurrentAccountAsync();
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