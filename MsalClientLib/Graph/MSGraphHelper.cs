using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using MsalClientLib;
using System.Net.Http.Headers;

namespace MSALClientLib.Graph;

public class MSGraphHelper
{
    public readonly MSGraphApiConfig MSGraphApiConfig;

    public MSALClientHelper MSALClient { get; }
    private GraphServiceClient _graphServiceClient;

    private readonly string[] GraphScopes;

    public MSGraphHelper(MSGraphApiConfig graphApiConfig, MSALClientHelper msalClientHelper)
    {
        if (msalClientHelper == null)
        {
            throw new ArgumentNullException(nameof(msalClientHelper));
        }

        MSGraphApiConfig = graphApiConfig;
        MSALClient = msalClientHelper;
        GraphScopes = MSGraphApiConfig.ScopesArray;
    }

    /// <summary>
    /// Calls the MS Graph /me endpoint
    /// </summary>
    public async Task<User> GetMeAsync()
    {
        if (_graphServiceClient == null)
        {
            await SignInAndInitializeGraphServiceClient();
        }

        User graphUser = null;

        // Call /me Api

        try
        {
            graphUser = await _graphServiceClient.Me.GetAsync();
        }
        catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
        {
            _graphServiceClient = await SignInAndInitializeGraphServiceClientPostCAE(ex);

            // Call the /me endpoint of Graph again with a fresh token
            graphUser = await _graphServiceClient.Me.GetAsync();
        }
        return graphUser;
    }

    /// <summary>
    /// Calls the MS Graph /me/photo endpoint
    /// </summary>
    public async Task<Stream> GetMyPhotoAsync()
    {
        if (_graphServiceClient == null)
        {
            await SignInAndInitializeGraphServiceClient();
        }

        Stream userPhoto = null;

        // Call /me/Photo Api

        try
        {
            userPhoto = await _graphServiceClient.Me.Photo.Content.GetAsync();
        }
        catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
        {
            _graphServiceClient = await SignInAndInitializeGraphServiceClientPostCAE(ex);

            // Call the /me endpoint of Graph again with a fresh token
            userPhoto = await _graphServiceClient.Me.Photo.Content.GetAsync();
        }
        return userPhoto;
    }

    public void ResetGraphClientService()
    {
        _graphServiceClient = null;
    }

    /// <summary>
    /// Sign in user using MSAL and obtain a token for MS Graph
    /// </summary>
    private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
    {
        string token = await MSALClient.SignInUserAndAcquireAccessToken(GraphScopes);
        return InitializeGraphServiceClientAsync(token);
    }

    private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClientPostCAE(ServiceException ex)
    {
        // Get challenge from response of Graph API
        var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(ex.ResponseHeaders);

        string token = await MSALClient.SignInUserAndAcquireAccessToken(GraphScopes, claimChallenge);
        return InitializeGraphServiceClientAsync(token);
    }

    private GraphServiceClient InitializeGraphServiceClientAsync(string token)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _graphServiceClient = new GraphServiceClient(client);

        return _graphServiceClient;
    }
}
