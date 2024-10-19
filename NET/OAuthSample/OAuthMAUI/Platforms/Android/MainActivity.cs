using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

using Microsoft.Identity.Client;
using Microsoft.Maui;

namespace OAuthMAUI;
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]

[IntentFilter(new[]
{
    Intent.ActionView
}, Categories = new[]
{
    Intent.CategoryDefault, Intent.CategoryBrowsable
}, DataScheme = "com.companyname.OAuthMAUI")]
public class MainActivity : MauiAppCompatActivity
{

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        // Return control to MSAL
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }
}