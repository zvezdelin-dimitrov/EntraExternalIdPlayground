using BlazorClient;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    // By default it uses popup
    options.ProviderOptions.LoginMode = builder.Configuration.GetValue<bool>("UseRedirectForLogin") ? "redirect" : "popup";

    // This usually is the email unless configured otherwise
    // By default it uses the "name" claim, which should be specifically added during registration or from an admin later
    options.UserOptions.NameClaim = "preferred_username";
});

builder.Services.AddHttpClient(Constants.ZvezdoApi, client => client.BaseAddress = new Uri(builder.Configuration["ZvezdoApiUrl"]))
    // This adds the access token with needed scope to every request
    .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>().ConfigureHandler([builder.Configuration["ZvezdoApiUrl"]], [builder.Configuration["ZvezdoApiRequiredScopes"]]));

builder.Services.AddHttpClient(Constants.GraphApi, client => client.BaseAddress = new Uri(builder.Configuration["GraphApiUrl"]))
    // This adds the access token with needed scope to every request
    .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>().ConfigureHandler([builder.Configuration["GraphApiUrl"]], [builder.Configuration["GraphApiRequiredScopes"]]));

await builder.Build().RunAsync();
