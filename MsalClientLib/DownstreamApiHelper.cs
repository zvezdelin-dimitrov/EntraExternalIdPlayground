namespace MsalClientLib;

public class DownstreamApiHelper
{
    private readonly string[] DownstreamApiScopes;
    public DownStreamApiConfig DownstreamApiConfig;
    private readonly MSALClientHelper MSALClient;

    public DownstreamApiHelper(DownStreamApiConfig downstreamApiConfig, MSALClientHelper msalClientHelper)
    {
        if (msalClientHelper == null)
        {
            throw new ArgumentNullException(nameof(msalClientHelper));
        }

        DownstreamApiConfig = downstreamApiConfig;
        MSALClient = msalClientHelper;
        DownstreamApiScopes = DownstreamApiConfig.ScopesArray;
    }
}
