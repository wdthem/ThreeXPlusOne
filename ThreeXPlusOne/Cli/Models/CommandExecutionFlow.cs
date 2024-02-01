namespace ThreeXPlusOne.Cli.Models;

public record CommandExecutionFlow()
{
    /// <summary>
    /// Whether or not processing should continue
    /// </summary>
    public bool Continue { get; set; }
}