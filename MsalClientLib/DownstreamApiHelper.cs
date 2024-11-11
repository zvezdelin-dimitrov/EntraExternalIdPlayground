using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsalClientLib
{
    public class DownstreamApiHelper
    {
        private string[] DownstreamApiScopes;
        public DownStreamApiConfig DownstreamApiConfig;
        private MSALClientHelper MSALClient;

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
}
