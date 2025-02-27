﻿using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IAlgorithmService : IScopedService
{
    /// <summary>
    /// Run the 3x+1 algorithm on each of the provided numbers.
    ///
    ///     The algorithm is:
    ///         Given a positive integer: if it is even, divide by 2; If it is odd, multiply by 3 and add 1.
    ///         Repeat until the calculated value is 1.
    /// </summary>
    /// <returns></returns>
    Task<List<CollatzResult>> Run();
}
