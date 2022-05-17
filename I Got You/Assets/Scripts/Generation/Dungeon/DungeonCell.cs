using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCell : MonoBehaviour
{
    public enum CellTypes { NONE, HALLWAY, ROOM}
    public CellTypes cellType = CellTypes.NONE;
    public int extraWallRemoval = -1;
}
