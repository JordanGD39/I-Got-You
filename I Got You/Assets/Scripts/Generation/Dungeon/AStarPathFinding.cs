using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathFinding : MonoBehaviour
{
    private DungeonGrid dungeonGrid;
    private DungeonCell startingCell;
    private DungeonCell targetCell;
    private List<GridNode> nodesToSearch;
    private List<GridNode> nodesProcessed;
    private List<DungeonCell> dungeonCellNodesCreated = new List<DungeonCell>();
    private List<GridNode> createdGridNodes = new List<GridNode>();

    private void SetGridNodeNeigbours(GridNode node)
    {
        Vector3 pos = node.dungeonCell.transform.position;
        AddIfValidNeighbour(pos + new Vector3(1, 0, 0), node);
        AddIfValidNeighbour(pos - new Vector3(1, 0, 0), node);
        AddIfValidNeighbour(pos + new Vector3(0, 0, 1), node);
        AddIfValidNeighbour(pos - new Vector3(0, 0, 1), node);
    }

    private void AddIfValidNeighbour(Vector3 pos3d, GridNode node)
    {
        Vector2Int pos = new Vector2Int((int)pos3d.x, (int)pos3d.z);

        DungeonCell cellValue;

        if (!dungeonGrid.Grid.TryGetValue(pos, out cellValue))
        {
            return;
        } 

        GridNode neighbour = CreateGridNode(cellValue);

        if (neighbour == null)
        {
            neighbour = createdGridNodes[dungeonCellNodesCreated.IndexOf(dungeonGrid.Grid[pos])];
        }

        if (dungeonGrid.Grid[pos].cellType == DungeonCell.CellTypes.ROOM)
        {
            neighbour.extraCost += 999;
        }

        node.neighbours.Add(neighbour);
    }

    private GridNode CreateGridNode(DungeonCell dungeonCell)
    {
        if (dungeonCellNodesCreated.Contains(dungeonCell))
        {
            return null;
        }

        GridNode node = new GridNode();
        node.dungeonCell = dungeonCell;
        node.g = GetDistanceToNode(dungeonCell, startingCell);
        node.h = GetDistanceToNode(dungeonCell, targetCell);

        dungeonCellNodesCreated.Add(dungeonCell);
        createdGridNodes.Add(node);

        return node;
    }

    private int GetDistanceToNode(DungeonCell start, DungeonCell target)
    {
        return Mathf.Abs((int)start.transform.position.x - (int)target.transform.position.x) + 
            Mathf.Abs((int)start.transform.position.z - (int)target.transform.position.z);
    }

    public List<GridNode> FindPath(DungeonCell start, DungeonCell target)
    {
        createdGridNodes.Clear();
        dungeonCellNodesCreated.Clear();

        startingCell = start;
        targetCell = target;

        nodesToSearch = new List<GridNode>();
        nodesProcessed = new List<GridNode>();

        if (dungeonGrid == null)
        {
            dungeonGrid = GetComponent<DungeonGrid>();
        }

        nodesToSearch.Add(CreateGridNode(start));
        GridNode startNode = nodesToSearch[0];

        int loopCounter = 0, maxLoops = 9999; //just an example

        while (nodesToSearch.Any())
        {
            if (loopCounter >= maxLoops)
            {
                Debug.LogError("INFINTE WHILE LOOP DETECTED!");
                break;
            }

            GridNode currentNode = nodesToSearch[0];

            foreach (GridNode node in nodesToSearch)
            {
                if (node.F < currentNode.F || node.F == currentNode.F && node.h < currentNode.h)
                {
                    currentNode = node;
                }
            }

            nodesProcessed.Add(currentNode);
            nodesToSearch.Remove(currentNode);

            if (currentNode.dungeonCell == targetCell)
            {
                GridNode currentPathTile = currentNode;

                List<GridNode> path = new List<GridNode>();

                while (currentPathTile != startNode)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.connection;
                }

                createdGridNodes.Clear();
                dungeonCellNodesCreated.Clear();
                nodesProcessed.Clear();
                nodesToSearch.Clear();

                return path;
            }

            if (currentNode.neighbours.Count == 0)
            {
                SetGridNodeNeigbours(currentNode);
            }

            foreach (GridNode neighbour in currentNode.neighbours)
            {
                if (neighbour == null || nodesProcessed.Contains(neighbour))
                {
                    continue;
                }

                //Encourages using the same hallway for multiple paths
                if (neighbour.dungeonCell.cellType == DungeonCell.CellTypes.NONE)
                {
                    neighbour.extraCost += 5;
                }

                bool inSearch = nodesToSearch.Contains(neighbour);

                if (!inSearch || currentNode.g + 1 < neighbour.g)
                {
                    neighbour.g = currentNode.g + 1;
                    neighbour.connection = currentNode;

                    if (!inSearch)
                    {
                        nodesToSearch.Add(neighbour);
                    }
                }
            }

            loopCounter++;
        }

        return null;
    }
}

[Serializable]
public class GridNode
{
    public DungeonCell dungeonCell;

    public GridNode connection;
    public List<GridNode> neighbours = new List<GridNode>();
    public int h = 0;
    public int g = 0;
    public int extraCost = 0;

    public int F { get { return h + g + extraCost; } }
}
