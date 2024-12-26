using System.Text.RegularExpressions;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Enums.Extensions;
using ThreeXPlusOne.App.Services.Interfaces;

namespace DrAggieThem.XeroIntegration.App.Services;

public partial class MarkupService : IMarkupService
{
    /// <summary>
    /// Regex to match markup but ignore ANSI escape codes.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"(\x1b[\[\]](?:[^\x07\\\]]*[\\\]]{0,1})*(?:\x07|\x1b\\)?)|(\[/\]|\[/?(?:color=)?[^\]]+\]|[^\[\]\x1b]+)")]
    private static partial Regex Markup();

    /// <summary>
    /// Stack to keep track of active formatting tags.
    /// </summary>
    private readonly Stack<(string tag, string ansiCode)> _activeFormatting = new();

    private static readonly string _formattingResetAnsiCodes = AnsiCode.Reset.GetCode() +
                                                               BaseColor.Background.ToAnsiCode() +
                                                               BaseColor.Foreground.ToAnsiCode();

    /// <summary>
    /// Get the ANSI code for a formatting tag.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    private static string? GetTextFormatAnsiCode(string tag)
    {
        string ansiFormat = tag.ToLower() switch
        {
            "b" or "bold" => AnsiCode.Bold.GetCode(),
            "i" or "italic" => AnsiCode.Italic.GetCode(),
            "u" or "underline" => AnsiCode.Underline.GetCode(),
            "dim" => AnsiCode.Dim.GetCode(),
            "hidden" => AnsiCode.Hidden.GetCode(),
            "highlight" => AnsiCode.Highlight.GetCode(),
            "strikethrough" => AnsiCode.Strikethrough.GetCode(),
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(ansiFormat))
        {
            return ansiFormat;
        }

        return null;
    }

    /// <summary>
    /// Combines color and text formatting operations for console output.
    /// </summary>
    private static string? GetMarkupAnsiCode(string tag)
    {
        // Try text formatting first
        string? formattingCode = GetTextFormatAnsiCode(tag);

        if (!string.IsNullOrEmpty(formattingCode))
        {
            return formattingCode;
        }

        // Try color markup
        if (!Enum.TryParse<AppColor>(tag, true, out var appColor))
        {
            return null;
        }

        return appColor.ToAnsiForegroundCode();
    }

    /// <summary>
    /// Process a markup tag and return the appropriate ANSI code and whether it's a closing tag.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    private (string ansiCode, bool isClosingTag) ProcessMarkupTag(string tag)
    {
        // Handle closing tags
        if (tag.StartsWith('/'))
        {
            string closingTag = tag[1..]; // Remove the '/' prefix

            if (string.IsNullOrEmpty(closingTag)) // [/] case - reset all
            {
                _activeFormatting.Clear();
                return (_formattingResetAnsiCodes, true);
            }

            // Find and remove the specific tag
            var tempStack = new Stack<(string tag, string ansiCode)>();
            bool foundTag = false;

            while (_activeFormatting.Count > 0)
            {
                var current = _activeFormatting.Pop();
                if (!foundTag && current.tag.Equals(closingTag, StringComparison.OrdinalIgnoreCase))
                {
                    foundTag = true;
                }
                else
                {
                    tempStack.Push(current);
                }
            }

            // Build the response string: reset + reapply remaining formatting
            string response = _formattingResetAnsiCodes;
            while (tempStack.Count > 0)
            {
                var format = tempStack.Pop();
                _activeFormatting.Push(format);
                response += format.ansiCode;
            }

            return (response, true);
        }

        // Handle opening tags
        string? ansiCode = GetMarkupAnsiCode(tag);

        if (ansiCode != null)
        {
            _activeFormatting.Push((tag, ansiCode));
            return (ansiCode, false);
        }

        return (tag, false);
    }

    /// <summary>
    /// Parse a message with markup and return a sequence of text and ANSI code pairs.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public IEnumerable<string> ParseMarkup(string message)
    {
        foreach (Match match in Markup().Matches(message))
        {
            if (!match.Value.StartsWith('['))
            {
                yield return match.Value;
                continue;
            }

            string tag = match.Value.Trim('[', ']');
            var (ansiCode, _) = ProcessMarkupTag(tag);

            yield return ansiCode;
        }
    }
}