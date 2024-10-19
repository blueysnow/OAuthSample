using System.Net;

using Microsoft.AspNetCore.Mvc;

namespace OAuthWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OAuthController : ControllerBase
{
    #region Private Field(s)

    private const string MobileCallBackProtcol = "oauthmaui";

    #endregion

    #region Endpoint(s)

    [HttpGet("{scheme}")]
    public void Get([FromRoute] string scheme)
    {
        var casedScheme = scheme.ToLower();
        switch (casedScheme)
        {
            case "google":
            case "dropbox":
                string? authUri = Request.Query["auth_uri"];
                Request.HttpContext.Response.Redirect(!string.IsNullOrWhiteSpace(authUri) ? authUri : $"{MobileCallBackProtcol}://#error_originator{scheme}&error=invalid_scheme");
                break;
            default:
                Request.HttpContext.Response.Redirect($"{MobileCallBackProtcol}://#error=invalid_scheme");
                break;
        }
    }

    [HttpGet("{scheme}/signin")]
    public void SignIn([FromRoute] string scheme)
    {
        var casedScheme = scheme.ToLower();
        switch (casedScheme)
        {
            case "google":
            case "dropbox":
                var mobileRedirect = $"{MobileCallBackProtcol}://#{string.Join("&",
                    Request.Query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value)).Select(q => $"{q.Key}={WebUtility.UrlEncode(q.Value)}"))}";
                Request.HttpContext.Response.Redirect(mobileRedirect);
                break;

            default:
                Request.HttpContext.Response.Redirect($"{MobileCallBackProtcol}://#error=invalid_scheme");
                break;
        }
    }

    #endregion
}
