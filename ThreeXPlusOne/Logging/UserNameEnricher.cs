using Serilog.Core;
using Serilog.Events;

namespace ThreeXPlusOne.Logging;

public class UserNameEnricher : ILogEventEnricher
{
    /// <summary>
    /// Prepend all log messages with the OS user name.
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyFactory"></param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string userName = GetCurrentUserName();
        LogEventProperty userNameProperty = propertyFactory.CreateProperty("UserName", userName);

        logEvent.AddPropertyIfAbsent(userNameProperty);
    }

    private static string GetCurrentUserName()
    {
        return Environment.UserName;
    }
}