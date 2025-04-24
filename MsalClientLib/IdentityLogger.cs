using Microsoft.IdentityModel.Abstractions;
using System.Diagnostics;

namespace MsalClientLib;

public class IdentityLogger : IIdentityLogger
{
    private readonly EventLogLevel _minLogLevel = EventLogLevel.LogAlways;

    public IdentityLogger(EventLogLevel minLogLevel = EventLogLevel.LogAlways)
    {
        _minLogLevel = minLogLevel;
    }

    public bool IsEnabled(EventLogLevel eventLogLevel)
    {
        return eventLogLevel >= _minLogLevel;
    }

    public void Log(LogEntry entry)
    {
        Debug.WriteLine($"MSAL: EventLogLevel: {entry.EventLogLevel}, Message: {entry.Message} ");
    }
}
