using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGrid : MonoBehaviour
{
    private Dictionary<Vector2Int, DungeonCell> grid = new Dictionary<Vector2Int, DungeonCell>();
    public Dictionary<Vector2Int, DungeonCell> Grid { get { return grid; } }

    [SerializeField] private Vector2Int gridSize;
    public Vector2Int GridSize { get { return gridSize; } }
    [SerializeField] private GameObject dungeonCellPrefab;
    
    public void GenerateGrid()
    {
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Vector3 posToGenerate = new Vector3(i, 0, j);
                GameObject dungeonCell = Instantiate(dungeonCellPrefab, posToGenerate, Quaternion.identity);
                grid.Add(new Vector2Int(i, j), dungeonCell.GetComponent<DungeonCell>());
            }
        }
    }

    public bool CheckIfRoomIsHere(Vector2Int pos, Vector3 scale)
    {
        for (int i = Mathf.FloorToInt(-scale.x); i < Mathf.CeilToInt(scale.x); i++)
        {
            for (int j = Mathf.FloorToInt(-scale.z); j < Mathf.CeilToInt(scale.z); j++)
            {
                Vector2Int checkGridPos = new Vector2Int(pos.x + i, pos.y + j);

                if (grid[checkGridPos].cellType == DungeonCell.CellTypes.ROOM)
                {
                    return true;
                } 
            }
        }

        return false;
    }

    public void SetGridCellsToType(DungeonCell.CellTypes cellType, Vector2Int pos, Vector3 scale)
    {
        for (int i = Mathf.FloorToInt(-scale.x); i < Mathf.CeilToInt(scale.x); i++)
        {
            for (int j = Mathf.FloorToInt(-scale.z); j < Mathf.CeilToInt(scale.z); j++)
            {
                Vector2Int checkGridPos = new Vector2Int(pos.x + i, pos.y + j);

                DungeonCell val;

                if (grid.TryGetValue(checkGridPos, out val))
                {
                    val.cellType = cellType;
                }                 
            }
        }
    }
}
