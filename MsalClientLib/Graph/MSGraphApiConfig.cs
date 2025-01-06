namespace MSALClientLib.Graph;

public class MSGraphApiConfig
{
    /// <summary>
    /// Gets or sets the scopes for MS graph call.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    public string Scopes { get; set; }

    /// <summary>
    /// Gets the scopes in a format as expected by the various MSAL SDK methods.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    public string[] ScopesArray
    {
        get
        {
            return Scopes.Split(' ');
        }
    }
}