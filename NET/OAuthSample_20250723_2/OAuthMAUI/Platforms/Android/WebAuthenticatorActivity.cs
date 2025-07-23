using Android.App;
using Android.Content;
using Android.Content.PM;

namespace OAuthMAUI;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[]
{
    Intent.ActionView
}, Categories = new[]
{
    Intent.CategoryDefault, Intent.CategoryBrowsable
}, DataScheme = "oauthmaui")]
internal class WebAuthenticatorActivity : WebAuthenticatorCallbackActivity
{
}