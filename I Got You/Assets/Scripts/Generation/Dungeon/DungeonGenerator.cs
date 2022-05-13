using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private DungeonGrid dungeonGrid;
    [SerializeField] private List<GameObject> roomPrefabs = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        dungeonGrid = GetComponent<DungeonGrid>();
        dungeonGrid.GenerateGrid();

        int roomCount = roomPrefabs.Count;
    }

    private void PlaceRoom()
    {
        int randRoom = Random.Range(0, roomPrefabs.Count);
        GameObject room = roomPrefabs[randRoom];
        Vector3 scale = room.transform.GetChild(0).localScale;

        int roundedX = Mathf.RoundToInt(scale.x);
        int roundedZ = Mathf.RoundToInt(scale.z);

        int randX = Random.Range(-dungeonGrid.GridSize.x + roundedX, dungeonGrid.GridSize.x - roundedX);
        int randZ = Random.Range(-dungeonGrid.GridSize.y + roundedZ, dungeonGrid.GridSize.y - roundedZ);

        Vector2Int randomGridPos = new Vector2Int(randX, randZ);

        bool roomHere = dungeonGrid.CheckIfRoomIsHere(randomGridPos, scale);

        if (roomHere)
        {
            PlaceRoom();
            return;
        }

        

        roomPrefabs.RemoveAt(randRoom);
    }
}
