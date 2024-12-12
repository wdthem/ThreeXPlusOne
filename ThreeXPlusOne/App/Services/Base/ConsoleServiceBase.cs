using System.Runtime.InteropServices;

namespace ThreeXPlusOne.App.Services.Base;

public abstract class ConsoleServiceBase
{
    protected ConsoleServiceBase()
    {
        EnsureAnsiSupport();
    }

    /// <summary>
    /// Ensure the console supports ANSI escape sequences.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException"></exception>
    private static void EnsureAnsiSupport()
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!EnableWindowsAnsi())
            {
                throw new PlatformNotSupportedException(
                    "This Windows terminal does not support ANSI escape sequences. Please use Windows Terminal or VS Code's integrated terminal.");
            }
        }
        else
        {
            var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");

            if (termProgram == "Apple_Terminal")
            {
                throw new PlatformNotSupportedException(
                    "The macOS Terminal.app does not fully support ANSI escape sequences. Please use iTerm2 or VS Code's integrated terminal instead.");
            }

            // Additional Unix-like system checks
            var term = Environment.GetEnvironmentVariable("TERM");
            if (string.IsNullOrEmpty(term))
            {
                throw new PlatformNotSupportedException(
                    "No terminal type detected. This application requires a terminal that supports ANSI escape sequences.");
            }
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr handle, out uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr handle, uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int handle);

    private static bool EnableWindowsAnsi()
    {
        nint handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE

        if (handle == IntPtr.Zero)
        {
            return false;
        }

        if (!GetConsoleMode(handle, out uint mode))
        {
            return false;
        }

        mode |= 0x0004; // ENABLE_VIRTUAL_TERMINAL_PROCESSING

        return SetConsoleMode(handle, mode);
    }
}