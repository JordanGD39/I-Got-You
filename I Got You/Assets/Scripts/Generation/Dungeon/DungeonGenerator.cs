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
    [SerializeField] private List<GameObject> rooms = new List<GameObject>();
    [SerializeField] private GameObject hallwayPrefab;
    [SerializeField] private Dictionary<Vector3, GameObject> roomsDictionary = new Dictionary<Vector3, GameObject>();
    [SerializeField] private List<DungeonCell> extraHallways = new List<DungeonCell>();
    public List<DungeonCell> ExtraHallways { get { return extraHallways; } }
    //private Delaunay2D delaunay;
    [SerializeField] private List<Vertex> vertices;
    private TriangleNet.Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        aStarPathFinding = GetComponent<AStarPathFinding>();
        primMinimumSpanningTree = GetComponent<PrimMinimumSpanningTree>();
        dungeonGrid = GetComponent<DungeonGrid>();
        dungeonGrid.GenerateGrid();

        int roomCount = roomPrefabs.Count;

        for (int i = 0; i < roomCount; i++)
        {
            PlaceRoom();
        }

        Triangulate();

        primMinimumSpanningTree.StartCreationOfMinimumSpanningTree(mesh);
        List<GridNode> allHallwayNodes = new List<GridNode>();

        foreach (PrimEdge edge in primMinimumSpanningTree.MinimumSpanningTree)
        {
            Vector3 pos = primMinimumSpanningTree.AllPoints[edge.startNode];
            Vector3 targetPos = primMinimumSpanningTree.AllPoints[edge.endNode];

            List<GridNode> gridNodes = 
                aStarPathFinding.FindPath(ChooseDoor(roomsDictionary[pos], targetPos), ChooseDoor(roomsDictionary[targetPos], pos));

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
            GameObject hallway = Instantiate(hallwayPrefab, node.dungeonCell.transform.position, Quaternion.identity);
            HallwayTile tile = hallway.GetComponent<HallwayTile>();
            tile.CheckSurroundings(dungeonGrid, this, false);
            tilesToRecheck.Add(tile);

            yield return new WaitForSeconds(0.05f);
        }

        foreach (DungeonCell cell in extraHallways)
        {
            GameObject hallway = Instantiate(hallwayPrefab, cell.transform.position, Quaternion.identity);
            hallway.GetComponent<HallwayTile>().CheckSurroundings(dungeonGrid, this, true);

            yield return new WaitForSeconds(0.05f);
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

        float lowestDistance = Mathf.Infinity;
        for (int i = 0; i < currentRoom.transform.GetChild(1).childCount; i++)
        {
            float distance = Vector3.Distance(currentRoom.transform.GetChild(1).GetChild(i).position, targetPos);

            if (distance < lowestDistance)
            {
                lowestDistance = distance;
                chosenDoor = currentRoom.transform.GetChild(1).GetChild(i);
            }
        }

        Vector2Int flooredPos = new Vector2Int(Mathf.FloorToInt(chosenDoor.position.x), Mathf.FloorToInt(chosenDoor.position.z));

        Debug.Log(flooredPos);

        return dungeonGrid.Grid[flooredPos];
    }

    private void PlaceRoom()
    {
        int randRoom = Random.Range(0, roomPrefabs.Count);
        GameObject roomPrefab = roomPrefabs[randRoom];
        Vector3 scale = roomPrefab.transform.GetChild(0).localScale;

        int roundedX = Mathf.RoundToInt(scale.x);
        int roundedZ = Mathf.RoundToInt(scale.z);

        int randX = Random.Range(roundedX, dungeonGrid.GridSize.x - roundedX);
        int randZ = Random.Range(roundedZ, dungeonGrid.GridSize.y - roundedZ);

        Vector2Int randomGridPos = new Vector2Int(randX, randZ);

        bool roomHere = dungeonGrid.CheckIfRoomIsHere(randomGridPos, scale);

        if (roomHere)
        {
            PlaceRoom();
            return;
        }

        Vector3 pos = new Vector3(randomGridPos.x, 0, randomGridPos.y);

        GameObject room = Instantiate(roomPrefab, pos, Quaternion.identity);
        dungeonGrid.SetGridCellsToType(DungeonCell.CellTypes.ROOM, randomGridPos, scale);
        rooms.Add(room);
        roomsDictionary.Add(pos, room);

        roomPrefabs.RemoveAt(randRoom);
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
}
