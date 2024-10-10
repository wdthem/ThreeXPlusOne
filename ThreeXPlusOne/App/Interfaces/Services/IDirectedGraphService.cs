using System.Diagnostics;

namespace ThreeXPlusOne.App.Interfaces.Services;

public interface IDirectedGraphService : IScopedService
{
    Task GenerateDirectedGraph(Stopwatch stopwatch);
}