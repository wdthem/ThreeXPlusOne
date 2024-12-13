using System.Diagnostics;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Helpers;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class ProgressIndicatorPresenter(IConsoleService consoleService) : IProgressIndicatorPresenter
{
    private int _spinnerCounter = 0;
    private const long _spinnerUpdateInterval = 120;
    private CancellationTokenSource? _cancellationTokenSource;
    private static readonly string[] _spinner = [
        EmojiHelper.GetEmojiUnicodeValue(Emoji.NewMoon),
        EmojiHelper.GetEmojiUnicodeValue(Emoji.WaxingCrescentMoon),
        EmojiHelper.GetEmojiUnicodeValue(Emoji.FirstQuarterMoon),
        EmojiHelper.GetEmojiUnicodeValue(Emoji.WaxingGibbousMoon),
        EmojiHelper.GetEmojiUnicodeValue(Emoji.FullMoon),
        EmojiHelper.GetEmojiUnicodeValue(Emoji.WaningGibbousMoon),
        EmojiHelper.GetEmojiUnicodeValue(Emoji.LastQuarterMoon),
        EmojiHelper.GetEmojiUnicodeValue(Emoji.WaningCrescentMoon)
    ];

    /// <summary>
    /// Write a spinning bar to the console in a threadsafe way to indicate an ongoing process.
    /// </summary>
    public async Task StartSpinner(string? message = null)
    {
        long previousMilliseconds = 0;

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        Stopwatch stopwatch = Stopwatch.StartNew();

        consoleService.SetCursorVisibility(false);
        consoleService.Write(ColorHelper.DefaultAnsiForegroundColour);

        if (message != null)
        {
            consoleService.Write($"{message}");
        }

        try
        {
            while (!token.IsCancellationRequested)
            {
                long currentMilliseconds = stopwatch.ElapsedMilliseconds;

                if (currentMilliseconds - previousMilliseconds >= _spinnerUpdateInterval)
                {
                    consoleService.Write(_spinner[_spinnerCounter]);
                    (int Left, int Top) = consoleService.GetCursorPosition();
                    consoleService.SetCursorPosition(Left - 2, Top);
                    _spinnerCounter = (_spinnerCounter + 1) % _spinner.Length;
                    previousMilliseconds = currentMilliseconds;
                }

                await Task.Delay(1, token);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation occurs
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Stop the spinning bar in a threadsafe way.
    /// </summary>
    public async Task StopSpinner()
    {
        if (_cancellationTokenSource == null)
        {
            return;
        }

        await _cancellationTokenSource.CancelAsync();
        (int Left, _) = consoleService.GetCursorPosition();
        if (Left == 2)
        {
            // If cursor is at position 2, the spinner is the only thing on the line
            // Clear the entire line
            consoleService.Write("\u001b[2K\r");
        }
        else
        {
            // Move cursor forward past the emoji, then delete it
            consoleService.Write("  \b\b\b\b  \b\b");
        }

        consoleService.SetCursorVisibility(true);

        _cancellationTokenSource = null;
    }
}