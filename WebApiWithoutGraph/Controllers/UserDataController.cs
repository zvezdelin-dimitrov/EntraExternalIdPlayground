using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using WebApiWithoutGraph.Dtos;

namespace WebApiWithoutGraph.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class UserDataController : ControllerBase
{
    [HttpGet(Name = "GetUserData")]
    public async Task<UserData> Get()
    {
        // This usually is the email
        var preferredNameFromClaim = User.Claims.SingleOrDefault(x => x.Type == "preferred_username")?.Value;

        // In order for this to work, the optional email claim for ACCESS TOKEN should be added in token configuration
        var emailFromClaim = User.Claims.SingleOrDefault(x => x.Type.EndsWith("emailaddress"))?.Value;

        return new UserData
        (
            preferredNameFromClaim,
            emailFromClaim
        );
    }
}
