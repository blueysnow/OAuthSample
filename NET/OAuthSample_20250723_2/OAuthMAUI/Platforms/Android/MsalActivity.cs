using Android.App;
using Android.Content;

using Microsoft.Identity.Client;

namespace OAuthMAUI;


[Activity(Exported = true)]
[IntentFilter(new[]
    {
        Intent.ActionView
    },
    Categories = new[]
    {
        Intent.CategoryBrowsable, Intent.CategoryDefault
    },
    DataHost = "auth",
    DataScheme = "msal15afedda-afd1-4b8b-a572-56df47e1211e")]
public class MsalActivity : BrowserTabActivity
{
}