using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private DungeonGrid dungeonGrid;
    private PrimMinimumSpanningTree primMinimumSpanningTree;

    [SerializeField] private List<GameObject> roomPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> rooms = new List<GameObject>();
    //private Delaunay2D delaunay;
    [SerializeField] private List<Vertex> vertices;
    private TriangleNet.Mesh mesh;
    private int maxEdgeIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        primMinimumSpanningTree = GetComponent<PrimMinimumSpanningTree>();
        dungeonGrid = GetComponent<DungeonGrid>();
        dungeonGrid.GenerateGrid();

        int roomCount = roomPrefabs.Count;

        for (int i = 0; i < roomCount; i++)
        {
            PlaceRoom();
        }

        Triangulate();

        primMinimumSpanningTree.CreateMinimumSpanningTree(mesh);

        InvokeRepeating(nameof(IncreaseMaxEdgeIndex), 0.5f, 0.5f);
    }

    private void IncreaseMaxEdgeIndex()
    {
        maxEdgeIndex++;
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

        GameObject room = Instantiate(roomPrefab, new Vector3(randomGridPos.x, 0, randomGridPos.y), Quaternion.identity);
        dungeonGrid.SetGridCellsToType(DungeonCell.CellTypes.ROOM, randomGridPos, scale);
        rooms.Add(room);

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


    public void OnDrawGizmosSelected()
    {
        if (mesh == null)
        {
            // We're probably in the editor
            return;
        }

        Gizmos.color = Color.green;
        int i = 0;

        foreach (Edge edge in mesh.Edges)
        {
            if (i > maxEdgeIndex)
            {
                break;
            }

            Vertex v0 = mesh.vertices[edge.P0];
            Vertex v1 = mesh.vertices[edge.P1];
            Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
            Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
            Gizmos.DrawLine(p0, p1);

            i++;
        }
    }
}
