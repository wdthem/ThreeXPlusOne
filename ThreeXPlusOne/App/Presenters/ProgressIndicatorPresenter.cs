using System.Diagnostics;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class ProgressIndicatorPresenter(IConsoleService consoleService) : IProgressIndicatorPresenter
{
    private int _spinnerCounter = 0;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string[] _spinner = ["|", "/", "-", "\\"];

    /// <summary>
    /// Write a spinning bar to the console in a threadsafe way to indicate an ongoing process.
    /// </summary>
    public async Task StartSpinner(string? message = null)
    {
        long previousMilliseconds = 0;
        const long updateInterval = 100;

        _cancellationTokenSource = new CancellationTokenSource();

        Stopwatch stopwatch = Stopwatch.StartNew();

        consoleService.SetCursorVisibility(false);

        if (message != null)
        {
            consoleService.WriteWithColorMarkup($"{message}");
        }

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            long currentMilliseconds = stopwatch.ElapsedMilliseconds;

            if (currentMilliseconds - previousMilliseconds >= updateInterval)
            {
                consoleService.WriteWithColorMarkup($"{_spinner[_spinnerCounter]}");
                consoleService.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                _spinnerCounter = (_spinnerCounter + 1) % _spinner.Length;
                previousMilliseconds = currentMilliseconds;
            }

            await Task.Delay(1, _cancellationTokenSource.Token);
        }

        stopwatch.Stop();
    }

    /// <summary>
    /// Stop the spinning bar in a threadsafe way.
    /// </summary>
    public async Task StopSpinner(string? message = null)
    {
        if (_cancellationTokenSource == null)
        {
            return;
        }

        await _cancellationTokenSource.CancelAsync();

        consoleService.SetCursorPosition(Console.CursorLeft - (message?.Length ?? 0), Console.CursorTop);
        consoleService.Write(new string(' ', message?.Length + 1 ?? 1));
        consoleService.SetCursorPosition(Console.CursorLeft - (message?.Length + 1 ?? 1), Console.CursorTop);

        consoleService.SetCursorVisibility(true);

        _cancellationTokenSource = null;
    }
}