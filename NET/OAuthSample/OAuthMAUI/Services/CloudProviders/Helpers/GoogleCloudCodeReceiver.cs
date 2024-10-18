using System.Net;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;

namespace OAuthMAUI.Services.CloudProviders.Helpers;
internal class GoogleCloudCodeReceiver : ICodeReceiver
{
    public string RedirectUri => "<insert-redirect-uri>";

    public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
    {
        try
        {
            // encode the url parameter and pass it as a query to the web api url
            var encodedAuthUrl = WebUtility.UrlEncode(url.Build().AbsoluteUri);

            var webApiUrl = $"https://eft-upward-bison.ngrok-free.app/mobileauth/google?auth_uri={encodedAuthUrl}";
            var response = await WebAuthenticator.AuthenticateAsync(new Uri(webApiUrl), new Uri("oauthmaui://"));

            return new AuthorizationCodeResponseUrl(response.Properties);
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

            return new AuthorizationCodeResponseUrl(response);
        }
    }
}
