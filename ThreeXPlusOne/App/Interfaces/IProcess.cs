namespace ThreeXPlusOne.App.Interfaces;

public interface IProcess
{
    /// <summary>
    /// Run the algorithm and data generation based on the user-provided settings
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    void Run(List<string> commandParsingMessages);
}