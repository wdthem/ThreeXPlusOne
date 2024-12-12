namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IDirectedGraphService : IScopedService
{
    Task GenerateDirectedGraph();
}