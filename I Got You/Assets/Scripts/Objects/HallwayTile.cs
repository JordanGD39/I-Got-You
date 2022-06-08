using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallwayTile : MonoBehaviour
{
    [SerializeField] private GameObject[] walls;
    private DungeonGrid grid;
    private DungeonGenerator dungeonGenerator;
    private bool extraWallPlaced = false;
    
    public void CheckSurroundings(DungeonGrid aGrid, DungeonGenerator gen, bool extra)
    {
        extraWallPlaced = extra;
        grid = aGrid;
        dungeonGenerator = gen;

        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        DungeonCell cell;

        if (!grid.Grid.TryGetValue(gridPos, out cell))
        {
            return;
        }

        cell.cellType = DungeonCell.CellTypes.HALLWAY;

        CheckPosForHallway(gridPos - new Vector2Int(1, 0), 0);
        CheckPosForHallway(gridPos + new Vector2Int(0, 1), 1);
        CheckPosForHallway(gridPos + new Vector2Int(1, 0), 2);
        CheckPosForHallway(gridPos - new Vector2Int(0, 1), 3);
    }

    private void CheckPosForHallway(Vector2Int posToCheck, int wallToRemove)
    {
        DungeonCell cell;

        if (!grid.Grid.TryGetValue(posToCheck, out cell))
        {
            return;
        }

        switch (cell.cellType)
        {
            case DungeonCell.CellTypes.NONE:
                if (!extraWallPlaced)
                {
                    extraWallPlaced = true;

                    grid.Grid[posToCheck].cellType = DungeonCell.CellTypes.HALLWAY;
                    dungeonGenerator.ExtraHallways.Add(grid.Grid[posToCheck]);
                    RemoveWall(wallToRemove);
                }
                break;
            case DungeonCell.CellTypes.HALLWAY:
                RemoveWall(wallToRemove);

                if (grid.Grid[posToCheck].extraWallRemoval >= 0)
                {
                    RemoveWall(grid.Grid[posToCheck].extraWallRemoval);
                }
                break;
            case DungeonCell.CellTypes.ROOM:
                break;
        }
    }

    public void RemoveWall(int wallIndex)
    {
        walls[wallIndex].SetActive(false);
    }
}
