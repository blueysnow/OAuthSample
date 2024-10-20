
using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;

using Dropbox.Api;

using Faithlife.Utility;

using OAuthShared;

using OAuthWinUI.Services.CloudProviders.Helpers;

namespace OAuthWinUI.Services.CloudProviders;

internal partial class DropBoxProvider : ObservableObject, ICloudProvider
{
    #region OAuthSettings

    private record SerializableOAuthResponse(string AccessToken, string Uid, string State, string TokenType, string RefreshToken, DateTime? ExpiresAt, string[] ScopeList);

    public static class DAuthenticationSettings
    {
        public const string AppKey = "<insert-app-key-here>";
        public const string AppSecret = "<insert-app-secret-here>";
        public const string DesktopRedirectUri = "oauthwinui://";
    }

    #endregion

    #region Properties

    private string? AuthenticationUrl
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

    #region Private Fields
   
    [ObservableProperty]
    private UserDetails currentUser = new();

    #endregion

    #region Constructor

    public DropBoxProvider()
    {
        AuthenticationUrl = default;
        AuthenticationResponse = default;
        Client = default;
    }

    #endregion

    #region Private Method(s)

    private DropboxClient CreateDropboxClient()
    {
        try
        {   ArgumentNullException.ThrowIfNull(AuthenticationResponse);
            ArgumentException.ThrowIfNullOrWhiteSpace(AuthenticationResponse.AccessToken);
            var expiresAt = AuthenticationResponse.ExpiresAt ?? DateTime.Now;
            return new DropboxClient(AuthenticationResponse.AccessToken, AuthenticationResponse.RefreshToken, expiresAt, DAuthenticationSettings.AppKey, DAuthenticationSettings.AppSecret);
        }

        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            throw;
        }
    }

    private string GenerateAuthenticationUrl(string localHost, out PKCEOAuthFlow pKceoAuthFlow)
    {
        try
        {
            OAuthToState = Guid.NewGuid().ToString("N");
            pKceoAuthFlow = new PKCEOAuthFlow();
            var authorizeUri = pKceoAuthFlow.GetAuthorizeUri(OAuthResponseType.Code, DAuthenticationSettings.AppKey, localHost, OAuthToState, tokenAccessType: TokenAccessType.Offline);
            return authorizeUri.AbsoluteUri;
        }
        catch (Exception exception)
        {

            Console.WriteLine("Obtaining DropBox Authentication URL Error " + exception.Message);
            throw;
        }
    }

    private async Task<OAuth2Response?> AuthorizeDropbox()
    {
        try
        {
            if (AuthenticationResponse is not null && string.IsNullOrWhiteSpace(AuthenticationResponse.AccessToken) == false)
            {
                return AuthenticationResponse;
            }
            
            DropBoxOAuthLocalServer webServer = new();
            AuthenticationUrl = GenerateAuthenticationUrl(webServer.RedirectUri, out var pKceoAuthFlow);
            UriBuilder dropBoxRedirectUri = new(webServer.RedirectUri);

            CancellationTokenSource oauthCancellationSource = new();
            var response = await WebAuthenticator.AuthenticateAsync(new Uri(AuthenticationUrl), new Uri(DAuthenticationSettings.DesktopRedirectUri), oauthCancellationSource.Token);

            var responseProps = response.Properties;
            var hasCodeKey = responseProps.TryGetValue("code", out var codeValue);
            var hasStateKey = responseProps.TryGetValue("state", out var stateValue);
            if (!hasCodeKey || !hasStateKey)
            {
                return AuthenticationResponse;
            }
            _ = dropBoxRedirectUri.AppendQuery($"code={codeValue}");
            _ = dropBoxRedirectUri.AppendQuery($"state={stateValue}");
            AuthenticationResponse = await pKceoAuthFlow.ProcessCodeFlowAsync(dropBoxRedirectUri.Uri, DAuthenticationSettings.AppKey, webServer.RedirectUri, OAuthToState);

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

    #endregion

    #region ICloudProvider Implementation

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

    public async Task<bool> SignInAsync()
    {
        AuthenticationResponse = await AuthorizeDropbox();
        if (AuthenticationResponse is not null)
        {
            Client = CreateDropboxClient();
        }
        if (Client is null)
        {
            return false;
        }
        var dropboxUser = await Client.Users.GetCurrentAccountAsync();
        CurrentUser = new UserDetails(dropboxUser.Name.DisplayName, dropboxUser.Name.DisplayName, dropboxUser.Name.GivenName, dropboxUser.Name.Surname, dropboxUser.Email);
        return true;
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

    #endregion
}
