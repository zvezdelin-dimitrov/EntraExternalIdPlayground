using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using WebApi.Dtos;

namespace WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class UserDataController : ControllerBase
{
    private readonly GraphServiceClient _graphServiceClient;

    public UserDataController(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    [HttpGet(Name = "GetUserData")]
    public async Task<UserData> Get()
    {
        // In order for this to work, the User.Read Microsoft Graph should be added in API permissions
        // A valid client secret should also be present in the appsettings (a possible workaround is impersonating the user, but it will require an additional request, more code and additional permissions) 
        var userFromGraph = await _graphServiceClient.Me.Request().GetAsync();

        // Should be added if needed during registration or from an admin later
        var displayNameFromGraph = userFromGraph.DisplayName;

        var emailFromGraph = userFromGraph.Mail;

        // This usually is the email
        var preferredNameFromClaim = User.Claims.SingleOrDefault(x => x.Type == "preferred_username")?.Value;

        // In order for this to work, the optional email claim for ACCESS TOKEN should be added in token configuration
        var emailFromClaim = User.Claims.SingleOrDefault(x => x.Type.EndsWith("emailaddress"))?.Value;

        return new UserData
        (
            displayNameFromGraph,
            emailFromGraph,
            preferredNameFromClaim,
            emailFromClaim
        );
    }
}
