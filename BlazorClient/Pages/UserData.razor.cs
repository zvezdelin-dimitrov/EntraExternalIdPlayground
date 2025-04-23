using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorClient.Pages;

[Authorize]
public partial class UserData
{
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    [Inject] private IHttpClientFactory ClientFactory { get; set; }

    private string Response { get; set; }

    private ClaimsPrincipal User { get; set; }

    protected override async Task OnInitializedAsync()
    {
        User = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
    }

    private async Task CallApi()
    {
        var httpClient = ClientFactory.CreateClient(Constants.ZvezdoApi);
        var response = await httpClient.GetAsync("userdata");
        Response = await response.Content.ReadAsStringAsync();
    }

    private async Task GetGraphData()
    {
        var httpClient = ClientFactory.CreateClient(Constants.GraphApi);
        var response = await httpClient.GetAsync("me");
        Response = await response.Content.ReadAsStringAsync();
    }
}
