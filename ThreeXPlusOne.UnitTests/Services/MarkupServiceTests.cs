using FluentAssertions;
using Xunit;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Services;
using ThreeXPlusOne.App.Enums.Extensions;

namespace ThreeXPlusOne.UnitTests.Services;

public class MarkupServiceTests
{
    private readonly MarkupService _markupService;
    private readonly string _resetCodes;

    public MarkupServiceTests()
    {
        _markupService = new MarkupService();
        _resetCodes = $"{AnsiCode.Reset.GetCode()}{BaseColor.Background.ToAnsiCode()}{BaseColor.Foreground.ToAnsiCode()}";
    }

    [Theory]
    [InlineData("[b]Bold[/b]", "Bold", AnsiCode.Bold)]
    [InlineData("[i]Italic[/i]", "Italic", AnsiCode.Italic)]
    [InlineData("[u]Underline[/u]", "Underline", AnsiCode.Underline)]
    public void GetDecoratedMessage_WithBasicFormatting_ReturnsCorrectAnsiCodes(string input, string text, AnsiCode code)
    {
        // Arrange
        var expected = $"{code.GetCode()}{text}{_resetCodes}";

        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDecoratedMessage_WithNestedFormatting_ReturnsCorrectAnsiCodes()
    {
        // Arrange
        var input = "[b]Bold[i]BoldItalic[/i]StillBold[/b]";
        var expected = $"{AnsiCode.Bold.GetCode()}Bold" +
                      $"{AnsiCode.Italic.GetCode()}BoldItalic" +
                      $"{_resetCodes}" +
                      $"{AnsiCode.Bold.GetCode()}StillBold" +
                      $"{_resetCodes}";

        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDecoratedMessage_WithColor_ReturnsCorrectAnsiCodes()
    {
        // Arrange
        var input = "[BlushRed]Colored Text[/BlushRed]";
        var expected = $"{AppColor.BlushRed.ToAnsiForegroundCode()}Colored Text" +
                      $"{AnsiCode.Reset.GetCode()}{BaseColor.Background.ToAnsiCode()}{BaseColor.Foreground.ToAnsiCode()}";

        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDecoratedMessage_WithColorAndFormatting_ReturnsCorrectAnsiCodes()
    {
        // Arrange
        var input = "[BlushRed][b]Bold Red Text[/b][/BlushRed]";
        var expected = $"{AppColor.BlushRed.ToAnsiForegroundCode()}" +
                      $"{AnsiCode.Bold.GetCode()}" +
                      "Bold Red Text" +
                      $"{AnsiCode.Reset.GetCode()}" +
                      $"{BaseColor.Background.ToAnsiCode()}" +
                      $"{BaseColor.Foreground.ToAnsiCode()}" +
                      $"{AppColor.BlushRed.ToAnsiForegroundCode()}" +
                      $"{AnsiCode.Reset.GetCode()}" +
                      $"{BaseColor.Background.ToAnsiCode()}" +
                      $"{BaseColor.Foreground.ToAnsiCode()}";

        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDecoratedMessage_WithGlobalReset_ResetsAllFormatting()
    {
        // Arrange
        var input = "[b][BlushRed]Formatted[/]Plain";
        var expected = $"{AnsiCode.Bold.GetCode()}{AppColor.BlushRed.ToAnsiForegroundCode()}Formatted" +
                      $"{AnsiCode.Reset.GetCode()}{BaseColor.Background.ToAnsiCode()}{BaseColor.Foreground.ToAnsiCode()}" +
                      "Plain";

        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDecoratedMessage_WithInvalidTag_PreservesOriginalText()
    {
        // Arrange
        var input = "[invalid]Text[/invalid]";
        var expected = "Text" +
                      $"{AnsiCode.Reset.GetCode()}{BaseColor.Background.ToAnsiCode()}{BaseColor.Foreground.ToAnsiCode()}";

        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetDecoratedMessage_WithPlainText_ReturnsUnmodifiedText()
    {
        // Arrange
        var input = "Plain text without markup";

        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(input);
    }

    [Theory]
    [InlineData(@"Text with \[literal brackets\]", "Text with [literal brackets]")]
    [InlineData(@"\[BlushRed\]Not red\[/BlushRed\]", "[BlushRed]Not red[/BlushRed]")]
    [InlineData(@"Allow literal backslash \ ", "Allow literal backslash \\ ")]
    public void GetDecoratedMessage_WithEscapedBrackets_HandlesThemCorrectly(string input, string expected)
    {
        // Act
        var result = _markupService.GetDecoratedMessage(input);

        // Assert
        result.Should().Be(expected);
    }
}