using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Enums.Extensions;

namespace ThreeXPlusOne.App.Helpers;

public static class HrefHelper
{
    /// <summary>
    /// Create an ANSI hyperlink.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="displayText"></param>
    /// <returns></returns>
    private static string CreateAnsiHyperlink(string uri, string displayText)
    {
        return AnsiCode.Hyperlink.GetCode(uri, displayText);
    }

    /// <summary>
    /// Get an ANSI hyperlink.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="displayText"></param>
    /// <returns></returns>
    public static string GetHyperlink(string url, string displayText)
    {
        return CreateAnsiHyperlink(url, displayText);
    }

    /// <summary>
    /// Get an ANSI hyperlink to a local file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="displayText"></param>
    /// <returns></returns>
    public static string GetLocalFileLink(string filePath, string displayText)
    {
        // Format path based on OS
        string formattedPath = OperatingSystem.IsWindows()
            ? filePath.Replace('\\', '/').TrimStart('/') // Windows: remove leading slash
            : filePath.Replace('\\', '/');               // Unix: keep leading slash

        // Add appropriate number of slashes based on OS
        string fileUrl = OperatingSystem.IsWindows()
            ? $"file:///{formattedPath}"    // Windows needs 3 slashes
            : $"file://{formattedPath}";    // Unix-like systems need 2 slashes

        return CreateAnsiHyperlink(fileUrl, displayText);
    }
}