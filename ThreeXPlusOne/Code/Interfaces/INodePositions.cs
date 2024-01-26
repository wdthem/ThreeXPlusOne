using ThreeXPlusOne.Code.Models;

namespace ThreeXPlusOne.Code.Interfaces;

public interface INodePositions
{
    /// <summary>
    /// The graph starts out at 0,0 with 0 width and 0 height. This means that nodes go into negative space as they are initially positioned, 
    /// so all coordinates need to be shifted to make sure all are in positive space
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="xNodeSpacer"></param>
    /// <param name="yNodeSpacer"></param>
    /// <param name="nodeRadius"></param>
    void MoveNodesToPositiveCoordinates(Dictionary<int, DirectedGraphNode> nodes,
                                        double xNodeSpacer,
                                        double yNodeSpacer,
                                        double nodeRadius);

    /// <summary>
    /// Determine if the node that was just positioned is too close to neighbouring nodes (and thus overlapping)
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    bool NodeIsTooCloseToNeighbours(DirectedGraphNode newNode,
                                    double minDistance);

    /// <summary>
    /// Add the node to the grid dictionary to keep track of node positions via a grid system
    /// </summary>
    /// <param name="node"></param>
    /// <param name="minDistance"></param>
    void AddNodeToGrid(DirectedGraphNode node,
                       double minDistance);
}