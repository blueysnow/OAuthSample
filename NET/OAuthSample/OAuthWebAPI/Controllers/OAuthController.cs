using System.Net;

using Microsoft.AspNetCore.Mvc;

namespace OAuthWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OAuthController : ControllerBase
{
	#region Private Field(s)

	private const string MobileCallBackProtcol = "oauthdemoapp://";

    #endregion

    #region Endpoint(s)

    [HttpGet("{scheme}")]
    public void Get([FromRoute] string scheme)
    {
        string casedScheme = scheme.ToLower();
        switch (casedScheme)
        {
            case "google":
            case "dropbox":
                string? authUri = Request.Query["auth_uri"];
                if (!string.IsNullOrWhiteSpace(authUri))
                {
                    Request.HttpContext.Response.Redirect(authUri);
                }
                else
                {
                    Request.HttpContext.Response.Redirect($"{MobileCallBackProtcol}://#error_originator{scheme}&error=invalid_scheme");
                }
                break;
            default:
                Request.HttpContext.Response.Redirect($"{MobileCallBackProtcol}://#error=invalid_scheme");
                break;
        }
    }

    [HttpGet("{scheme}/signin")]
    public void SignIn([FromRoute] string scheme)
    {
        string casedScheme = scheme.ToLower();
        switch (casedScheme)
        {
            case "google":
                string mobileRedirect = $"{MobileCallBackProtcol}://#{string.Join("&",
                    Request.Query.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value)).Select(q => $"{q.Key}={WebUtility.UrlEncode(q.Value)}"))}";
                Request.HttpContext.Response.Redirect(mobileRedirect);
                break;
            case "dropbox":
                mobileRedirect = $"{MobileCallBackProtcol}://";
                Request.HttpContext.Response.Redirect(mobileRedirect);
                break;
            default:
                Request.HttpContext.Response.Redirect($"{MobileCallBackProtcol}://#error=invalid_scheme");
                break;
        }
    }

    #endregion
}
