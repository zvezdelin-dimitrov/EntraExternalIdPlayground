using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace MauiClient.Platforms.Android;

[Activity(Exported = true)]
[IntentFilter([Intent.ActionView],
    Categories = [Intent.CategoryBrowsable, Intent.CategoryDefault],
    DataHost = "auth",
    // TODO: Check if it is possible NOT to hardcode this
    DataScheme = "msal5861191c-aba9-45d8-90e7-8b64da4ee00a")]
public class MsalActivity : BrowserTabActivity
{
}
