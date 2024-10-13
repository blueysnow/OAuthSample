using System.Diagnostics;
using System.Net;

using Faithlife.Utility;

namespace OAuthWinUI.Services.CloudProviders.Helpers;
internal class DropBoxOAuthLocalServer : IDisposable
{
    #region Private Field(s)

    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly HttpListener listener;

    #endregion

    #region Public Property(ies)

    private string Url
    {
        get;
    }

    public string RedirectUri
    {
        get;
    }

    #endregion

    #region Constructor(s)

    public DropBoxOAuthLocalServer()
    {
        listener = new HttpListener();
        var port = FindAvailablePort();
        Url = $"http://localhost:{port}/";
        RedirectUri = $"{Url}authorize";
        listener.Prefixes.Add(Url);
        listener.Start();
        _ = Task.Run(WebServer);
    }

    #endregion

    #region Private Method(s)

    private async void WebServer()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            try
            {

                var context = await listener.GetContextAsync();

                if (context.Request.Url!.AbsolutePath != new Uri(RedirectUri).AbsolutePath
                    || context.Request.Url.Authority != new Uri(RedirectUri).Authority)
                {
                    continue;
                }

                var redirectUri = context.Request.Url;
                if (redirectUri is null)
                {
                    continue;
                }
                var queryStrings = redirectUri.Query[1..].Split('&').Select(x => x.Split('=')).ToDictionary(x => Uri.UnescapeDataString(x[0]), x => Uri.UnescapeDataString(x[1]));

                var responseUri = new UriBuilder("oauthdemoapp://");
                foreach (var query in queryStrings)
                {
                    Debug.WriteLine($"{query.Key} = {query.Value}");
                    _ = responseUri.AppendQuery($"{query.Key}={Uri.EscapeDataString(query.Value)}");
                }

                await using StreamWriter writer = new(context.Response.OutputStream);
                var streamString = $"""
				                       <html><head><meta http-equiv="Refresh" content="0; URL={responseUri}" /></head>
				                       <body><div style="border-width:1px;border-style: solid;align:center;padding:30px;margin:20px;background-color:#eee;width:300px">
				                       Received verification code. You may now close this window.</div></body></html>
				                       """;
                await writer.WriteLineAsync(streamString);
                await writer.WriteLineAsync("</form></div>");
                await writer.WriteLineAsync("<hr/><b>Values sent:</b><br/>");
                await writer.WriteLineAsync("</body></html>");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
                throw;
            }
        }
    }

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        listener.Stop();
    }

    private static int FindAvailablePort()
    {
        int[] ports = [60356, 60357, 60358, 60359];
        var port = ports.ElementAt(new Random().Next(0, ports.Length));
        return port;
    }

    #endregion
}
