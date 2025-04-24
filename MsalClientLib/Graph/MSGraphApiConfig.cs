namespace MSALClientLib.Graph;

public class MSGraphApiConfig
{
    public string Scopes { get; set; }

    public string[] ScopesArray => Scopes.Split(' ');
}