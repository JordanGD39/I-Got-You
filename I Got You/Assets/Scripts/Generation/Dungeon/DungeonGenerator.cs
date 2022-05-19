using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private DungeonGrid dungeonGrid;
    private PrimMinimumSpanningTree primMinimumSpanningTree;
    private AStarPathFinding aStarPathFinding;

    [SerializeField] private List<GameObject> roomPrefabs = new List<GameObject>();
    [SerializeField] private List<GenerationRoomData> rooms = new List<GenerationRoomData>();
    [SerializeField] private GameObject hallwayPrefab;
    [SerializeField] private Dictionary<Vector3, GameObject> roomsDictionary = new Dictionary<Vector3, GameObject>();
    [SerializeField] private List<Vector3> placedHallwaysPos = new List<Vector3>();
    [SerializeField] private List<DungeonCell> extraHallways = new List<DungeonCell>();
    public List<DungeonCell> ExtraHallways { get { return extraHallways; } }
    //private Delaunay2D delaunay;
    [SerializeField] private List<Vertex> vertices;
    [SerializeField] private float seperationDistanceMultiplier = 1.5f;
    [SerializeField] private float radius = 0;
    [SerializeField] private Vector3 roomPos;
    private float padding = 5;
    private TriangleNet.Mesh mesh;

    private int currentWallToRemove = -1;

    // Start is called before the first frame update
    void Start()
    {
        //Plane padding is the scale * 10 / 2
        padding = 10 / 2;

        aStarPathFinding = GetComponent<AStarPathFinding>();
        primMinimumSpanningTree = GetComponent<PrimMinimumSpanningTree>();
        dungeonGrid = GetComponent<DungeonGrid>();
        dungeonGrid.GenerateGrid();

        int roomCount = roomPrefabs.Count;

        for (int i = 0; i < roomCount; i++)
        {
            PlaceRoom();
        }

        SeperateRooms();

        foreach (GenerationRoomData room in rooms)
        {
            room.transform.position = new Vector3(Mathf.RoundToInt(room.transform.position.x), 
                0, Mathf.RoundToInt(room.transform.position.z));

            roomsDictionary.Add(room.transform.position, room.gameObject);

            dungeonGrid.SetGridCellsToType(DungeonCell.CellTypes.ROOM, 
                new Vector2Int(Mathf.RoundToInt(room.transform.position.x), Mathf.RoundToInt(room.transform.position.z)), room.RoomScaleObject.localScale * padding);

            foreach (Transform opening in room.Openings)
            {
                for (int i = 0; i < opening.childCount; i++)
                {
                    Vector3 pos = opening.GetChild(i).transform.position;

                    DungeonCell val;

                    if (dungeonGrid.Grid.TryGetValue(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)), out val))
                    {
                        val.cellType = DungeonCell.CellTypes.NONE;
                    }
                }
            }

            foreach (Transform cell in room.CellsToMakeRoomForOpening)
            {
                Vector3 pos = cell.transform.position;
                DungeonCell val = null;

                if (dungeonGrid.Grid.TryGetValue(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)), out val))
                {
                    val.cellType = DungeonCell.CellTypes.NONE;
                }               
            }
        }

        Triangulate();

        primMinimumSpanningTree.StartCreationOfMinimumSpanningTree(mesh);

        List<GridNode> allHallwayNodes = new List<GridNode>();

        foreach (PrimEdge edge in primMinimumSpanningTree.MinimumSpanningTree)
        {
            Vector3 pos = primMinimumSpanningTree.AllPoints[edge.startNode];
            Vector3 targetPos = primMinimumSpanningTree.AllPoints[edge.endNode];

            List<GridNode> gridNodes = 
                aStarPathFinding.FindPath(ChooseDoor(roomsDictionary[pos], targetPos), 
                ChooseDoor(roomsDictionary[targetPos], pos), currentWallToRemove);

            if (gridNodes != null)
            {
                allHallwayNodes.AddRange(gridNodes);

                foreach (GridNode node in gridNodes)
                {
                    node.dungeonCell.cellType = DungeonCell.CellTypes.HALLWAY;
                }
            }
        }

        StartCoroutine(SlowdownHallwayCreation(allHallwayNodes));

        //foreach (GridNode node in allHallwayNodes)
        //{
        //    GameObject hallway = Instantiate(hallwayPrefab, node.dungeonCell.transform.position, Quaternion.identity);
        //}
        //List<GridNode> gridNodes = aStarPathFinding.FindPath();
    }

    private IEnumerator SlowdownHallwayCreation(List<GridNode> allHallwayNodes)
    {
        while (!primMinimumSpanningTree.ShowMST)
        {
            yield return null;
        }

        allHallwayNodes = allHallwayNodes.Distinct().ToList();

        List<HallwayTile> tilesToRecheck = new List<HallwayTile>();

        foreach (GridNode node in allHallwayNodes)
        {
            if (placedHallwaysPos.Contains(node.dungeonCell.transform.position))
            {
                continue;
            }

            GameObject hallway = Instantiate(hallwayPrefab, node.dungeonCell.transform.position, Quaternion.identity);
            HallwayTile tile = hallway.GetComponent<HallwayTile>();
            tile.CheckSurroundings(dungeonGrid, this, false);

            if (node.dungeonCell.extraWallRemoval >= 0)
            {
                tile.RemoveWall(node.dungeonCell.extraWallRemoval);
            }

            tilesToRecheck.Add(tile);
            placedHallwaysPos.Add(node.dungeonCell.transform.position);

            yield return new WaitForSeconds(0.05f);
        }

        foreach (DungeonCell cell in extraHallways)
        {
            if (placedHallwaysPos.Contains(cell.transform.position))
            {
                continue;
            }

            GameObject hallway = Instantiate(hallwayPrefab, cell.transform.position, Quaternion.identity);
            hallway.GetComponent<HallwayTile>().CheckSurroundings(dungeonGrid, this, true);
            placedHallwaysPos.Add(cell.transform.position);

            yield return new WaitForSeconds(0.05f);
        }

        foreach (GenerationRoomData generationRoomData in rooms)
        {
            foreach (Transform opening in generationRoomData.ChosenOpenings)
            {
                if (placedHallwaysPos.Contains(opening.GetChild(1).transform.position))
                {
                    continue;
                }

                GameObject hallway = Instantiate(hallwayPrefab, opening.GetChild(1).transform.position, Quaternion.identity);
                hallway.GetComponent<HallwayTile>().CheckSurroundings(dungeonGrid, this, true);
                placedHallwaysPos.Add(opening.GetChild(1).transform.position);

                yield return new WaitForSeconds(0.05f);
            }            
        }

        foreach (HallwayTile tile in tilesToRecheck)
        {
            tile.CheckSurroundings(dungeonGrid, this, true);
        }

        extraHallways.Clear();

        foreach (var item in dungeonGrid.Grid)
        {
            Destroy(item.Value.gameObject);
        }

        dungeonGrid.Grid.Clear();
    }

    private DungeonCell ChooseDoor(GameObject currentRoom, Vector3 targetPos)
    {
        Transform chosenDoor = null;
        GenerationRoomData generationRoomData = currentRoom.GetComponent<GenerationRoomData>();

        float lowestDistance = Mathf.Infinity;
        int chosenIndex = -1;

        for (int i = 0; i < generationRoomData.Openings.Length; i++)
        {
            float distance = Vector3.Distance(generationRoomData.Openings[i].GetChild(0).position, targetPos);

            if (distance < lowestDistance)
            {
                lowestDistance = distance;
                chosenDoor = generationRoomData.Openings[i].GetChild(0);
                chosenIndex = i;
            }
        }

        currentWallToRemove = generationRoomData.WallToRemove[chosenIndex];

        Vector2Int flooredPos = new Vector2Int(Mathf.FloorToInt(chosenDoor.position.x), Mathf.FloorToInt(chosenDoor.position.z));
        Debug.Log(chosenDoor.transform.position + " " + flooredPos +  " " + dungeonGrid.Grid[flooredPos].gameObject);
        generationRoomData.ChosenOpenings.Add(generationRoomData.Openings[chosenIndex]);

        dungeonGrid.Grid[flooredPos].extraWallRemoval = currentWallToRemove;

        return dungeonGrid.Grid[flooredPos];
    }

    private void PlaceRoom()
    {
        int randRoom = Random.Range(0, roomPrefabs.Count);
        GameObject roomPrefab = roomPrefabs[randRoom];
        Transform scaleObject = roomPrefab.GetComponent<GenerationRoomData>().RoomScaleObject;

        int roundedX = Mathf.RoundToInt(scaleObject.localScale.x * padding);
        int roundedZ = Mathf.RoundToInt(scaleObject.localScale.z * padding);

        int randX = Random.Range(roundedX, dungeonGrid.GridSize.x - roundedX);
        int randZ = Random.Range(roundedZ, dungeonGrid.GridSize.y - roundedZ);

        Vector2Int randomGridPos = new Vector2Int(randX, randZ);

        //bool roomHere = dungeonGrid.CheckIfRoomIsHere(randomGridPos, scaleObject.localScale);

        //if (roomHere)
        //{
        //    PlaceRoom();
        //    return;
        //}

        Vector3 pos = new Vector3(randomGridPos.x, 0, randomGridPos.y);

        GameObject room = Instantiate(roomPrefab, pos, Quaternion.identity);
        //dungeonGrid.SetGridCellsToType(DungeonCell.CellTypes.ROOM, randomGridPos, scaleObject.localScale);
        rooms.Add(room.GetComponent<GenerationRoomData>());

        roomPrefabs.RemoveAt(randRoom);
    }

    private void SeperateRooms()
    {
        int loopCounter = 0, maxLoops = 9999; //just an example

        while (RoomsOverlap())
        {
            if (loopCounter >= maxLoops)
            {
                Debug.LogError("INFINTE WHILE LOOP DETECTED!");
                break;
            }

            foreach (GenerationRoomData room in rooms)
            {
                float minDistance = Mathf.Max(room.RoomScaleObject.localScale.x, room.RoomScaleObject.localScale.z) * padding * seperationDistanceMultiplier;
                radius = minDistance;

                foreach (GenerationRoomData otherRoom in rooms)
                {
                    if (room == otherRoom)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(room.transform.position, otherRoom.transform.position);

                    if (distance < minDistance)
                    {
                        Vector3 dirToOtherRoom = room.transform.position - otherRoom.transform.position;

                        if (dirToOtherRoom == Vector3.zero)
                        {
                            dirToOtherRoom = Vector3.forward;
                        }

                        roomPos = room.transform.position;

                        room.transform.position += dirToOtherRoom;

                        float clampedX = Mathf.Clamp(room.transform.position.x, room.RoomScaleObject.localScale.x * padding,
                            dungeonGrid.GridSize.x - room.RoomScaleObject.localScale.x * padding);

                        float clampedZ = Mathf.Clamp(room.transform.position.z, room.RoomScaleObject.localScale.z * padding,
                            dungeonGrid.GridSize.y - room.RoomScaleObject.localScale.z * padding);

                        if (Vector3.Distance(room.oldPos, room.transform.position) < 2)
                        {
                            room.StuckTimes++;

                            if (room.StuckTimes > 2)
                            {
                                int roundedX = Mathf.RoundToInt(room.RoomScaleObject.localScale.x * padding);
                                int roundedZ = Mathf.RoundToInt(room.RoomScaleObject.localScale.z * padding);

                                int randX = Random.Range(roundedX, dungeonGrid.GridSize.x - roundedX);
                                int randZ = Random.Range(roundedZ, dungeonGrid.GridSize.y - roundedZ);

                                room.transform.position = new Vector3(randX, 0, randZ);
                                room.oldPos = room.transform.position;

                                room.StuckTimes = 0;
                                continue;
                            }
                        }
                        else
                        {
                            room.StuckTimes = 0;
                        }

                        room.transform.position = new Vector3(clampedX, 0, clampedZ);

                        room.oldPos = room.transform.position;
                        Debug.Log(room.transform.position);
                    }
                }
            }

            loopCounter++;
        }        
    }

    private bool RoomsOverlap()
    {
        foreach (GenerationRoomData room in rooms)
        {
            foreach (GenerationRoomData otherRoom in rooms)
            {
                if (otherRoom == room)
                {
                    continue;
                }

                float minDistance = Mathf.Max(room.RoomScaleObject.localScale.x, room.RoomScaleObject.localScale.z) * padding * seperationDistanceMultiplier;

                float distance = Vector3.Distance(room.transform.position, otherRoom.transform.position);

                //Debug.Log(distance + " dist " + minDistance);

                if (Mathf.Round(distance) < minDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void Triangulate()
    {
        Polygon polygon = new Polygon();

        for (int i = 0; i < rooms.Count; i++)
        {
            polygon.Add(new Vertex(rooms[i].transform.position.x, rooms[i].transform.position.z));
        }

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = false };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(roomPos, radius);
    }
}
