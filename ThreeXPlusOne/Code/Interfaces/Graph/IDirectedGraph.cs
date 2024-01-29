﻿namespace ThreeXPlusOne.Code.Interfaces.Graph;

public interface IDirectedGraph
{
    /// <summary>
    /// The number of dimensions the directed graph will render in
    /// </summary>
    int Dimensions { get; }

    /// <summary>
    /// Add multiple series of numbers generated by the algorithm to the directed graph
    /// </summary>
    /// <param name="seriesLists"></param>
    void AddSeries(List<List<int>> seriesLists);

    /// <summary>
    /// Position the nodes on the directed graph bassed on the provided settings
    /// </summary>
    void PositionNodes();

    /// <summary>
    /// Set the shape of each node to either circle or polygon, depending on settings
    /// </summary>
    void SetNodeShapes();

    /// <summary>
    /// Assign sizes to the canvas width and height after having positioned the nodes
    /// </summary>
    void SetCanvasDimensions();

    /// <summary>
    /// Generate a visual representation of the directed graph based on the settings
    /// </summary>
    void Draw();
}