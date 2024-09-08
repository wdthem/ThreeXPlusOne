namespace ThreeXPlusOne.App.Models;

/// <summary>
/// A record capturing the generated values from the algorithm along with related metadata.
/// </summary>
/// <remarks>
/// See https://en.wikipedia.org/wiki/Collatz_conjecture for more detail.
/// </remarks>
public record CollatzResult
{
    /// <summary>
    /// The starting value (first number in the series) 
    /// </summary>
    public int StartingValue { get; set; }

    /// <summary>
    /// The integer values generated by running the algorithm, including the <see cref="StartingValue"/> at the first index.
    /// </summary>
    public List<int> Values { get; set; } = [];

    /// <summary>
    /// The number of steps it takes to get a number smaller than the original number for the first time.
    /// </summary>
    public int StoppingTime
    {
        get
        {
            return Values.FindIndex(v => v < StartingValue);
        }
    }

    /// <summary>
    /// The total number of steps it takes to get to 1.
    /// </summary>
    /// <remarks>
    /// Note: Subtract one from the count of <see cref="Values"/> given it contains the <see cref="StartingValue"/>.
    /// </remarks>
    public int TotalStoppingTime
    {
        get
        {
            return Values.Count - 1;
        }
    }
}