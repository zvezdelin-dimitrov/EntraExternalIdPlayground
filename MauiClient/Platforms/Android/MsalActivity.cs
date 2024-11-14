using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace MauiClient.Platforms.Android;

[Activity(Exported = true)]
[IntentFilter([Intent.ActionView],
    Categories = [Intent.CategoryBrowsable, Intent.CategoryDefault],
    DataHost = "auth",
    DataScheme = "msal77a20c28-2f64-4ccb-857c-e47a0f609666")]
public class MsalActivity : BrowserTabActivity
{
}
