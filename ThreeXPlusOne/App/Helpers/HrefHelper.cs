namespace ThreeXPlusOne.App.Helpers;

public static class HrefHelper
{
    /// <summary>
    /// Get an ANSI hyperlink.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static string GetAnsiHyperlink(string url, string title)
    {
        return $"\x1b]8;;{url}\x07{title}\x1b]8;;\x07";
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

        return $"\u001b]8;;{fileUrl}\u0007{displayText}\u001b]8;;\u0007";
    }
}