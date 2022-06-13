using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class DungeonGenerator : MonoBehaviourPun
{
    private DungeonGrid dungeonGrid;
    private PrimMinimumSpanningTree primMinimumSpanningTree;
    private AStarPathFinding aStarPathFinding;

    [SerializeField] private Vector2Int lootRoomRange;
    [SerializeField] private Vector2Int arenaRange;
    [SerializeField] private List<GameObject> safeRoomPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> arenaPrefabs = new List<GameObject>();
    [SerializeField] private GameObject eatRoomPrefab;
    [SerializeField] private float eatGenChance = 25;
    [SerializeField] private List<GameObject> chosenRoomPrefabs = new List<GameObject>();
    [SerializeField] private List<GenerationRoomData> rooms = new List<GenerationRoomData>();
    [SerializeField] private GameObject hallwayPrefab;
    [SerializeField] private Dictionary<Vector3, GameObject> roomsDictionary = new Dictionary<Vector3, GameObject>();
    [SerializeField] private List<Vector3> placedHallwaysPos = new List<Vector3>();
    [SerializeField] private List<DungeonCell> extraHallways = new List<DungeonCell>();
    public List<DungeonCell> ExtraHallways { get { return extraHallways; } }
    //private Delaunay2D delaunay;
    [SerializeField] private List<Vertex> vertices;
    [SerializeField] private float seperationDistanceMultiplier = 1.5f;
    [SerializeField] private float seperationMultiplier = 2f;
    [SerializeField] private float distanceBetweenElevator = 5;
    [SerializeField] private RoomManager.RoomModes[] roomModesChances = { RoomManager.RoomModes.BATTLEONLY, RoomManager.RoomModes.BATTLEONLY, RoomManager.RoomModes.PUZZLECLOCK, RoomManager.RoomModes.PUZZLESIMON};
    private float radius = 0;
    private Vector3 roomPos;
    private float padding = 5;
    private TriangleNet.Mesh mesh;
    private int currentWallToRemove = -1;

    private GenerationRoomData randomChosenEndingRoom;
    private int randomChosenOpeningIndex = -1;

    public delegate void GenerationDone();
    public GenerationDone OnGenerationDone;
    public GenerationRoomData StartingRoom { get; private set; }

    public delegate void SeedChosen();
    public SeedChosen OnSeedChosen;
    private int seed = 0;
    private bool seedChosen = false;

    public delegate void HallwaysMade();
    public HallwaysMade OnHallwaysMade;
    private bool hallwaysMade = false;

    private List<Player> playersToSendDataTo = new List<Player>();
    private List<Player> playersToSendDataToHallway = new List<Player>();
    private List<Vector2> tilesToSend;

    // Start is called before the first frame update
    void Start()
    {
        //Plane padding is the scale * 10 / 2
        padding = 10 / 2;

        aStarPathFinding = GetComponent<AStarPathFinding>();
        primMinimumSpanningTree = GetComponent<PrimMinimumSpanningTree>();
        dungeonGrid = GetComponent<DungeonGrid>();

        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            ChooseLayout();

            dungeonGrid.GenerateGrid();

            int roomCount = chosenRoomPrefabs.Count;

            for (int i = 0; i < roomCount; i++)
            {
                PlaceRoom();                
            }

            PlaceEnd();

            SeperateRooms();            

            int ticks = (int)System.DateTime.Now.Ticks;
            Random.InitState(ticks);
            seed = ticks;
            //Debug.LogError("Seed: " + ticks);
            seedChosen = true;

            OnSeedChosen?.Invoke();

            StartGeneration();
        }

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RequestGenerationData", RpcTarget.MasterClient, (byte)PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    private void ChooseLayout()
    {
        int grid = 100;

        chosenRoomPrefabs.Add(safeRoomPrefabs[0]);

        float randEat = Random.Range(0, 100);

        if (randEat < eatGenChance)
        {
            chosenRoomPrefabs.Add(safeRoomPrefabs[1]);
            chosenRoomPrefabs.Add(eatRoomPrefab);
            //dungeonGrid.GridSize = new Vector2Int(grid, grid);
            return;
        }

        int rand = Random.Range(lootRoomRange.x, lootRoomRange.y);

        for (int i = 0; i < rand; i++)
        {
            chosenRoomPrefabs.Add(safeRoomPrefabs[1]);
            grid += 10;
        }

        rand = Random.Range(arenaRange.x, arenaRange.y);

        for (int i = 0; i < rand; i++)
        {
            chosenRoomPrefabs.Add(arenaPrefabs[Random.Range(0, arenaPrefabs.Count)]);
            grid += 30;
        }

        GameManager.instance.TotalArenaRooms += rand;
        dungeonGrid.GridSize = new Vector2Int(grid, grid);
    }

    [PunRPC]
    void RequestGenerationData(byte playerIndex)
    {
        playersToSendDataTo.Add(PhotonNetwork.PlayerList[playerIndex - 1]);

        if (seedChosen)
        {
            SendGenerationDataToOthers();
        }
        else
        {
            OnSeedChosen += SendGenerationDataToOthers;
        }
    }

    private void SendGenerationDataToOthers()
    {
        foreach (Player player in playersToSendDataTo)
        {
            foreach (GenerationRoomData room in rooms)
            {
                photonView.RPC("PlaceRoomOthers", player,
                    room.gameObject.GetPhotonView().ViewID, new Vector2(room.transform.position.x, room.transform.position.z));
            }

            photonView.RPC("PlaceEndOthers", player, rooms.IndexOf(randomChosenEndingRoom), randomChosenOpeningIndex);

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("StartGenerationForOthers", player, seed, dungeonGrid.GridSize.x);
            }
        }

        playersToSendDataTo.Clear();
    }

    [PunRPC]
    void StartGenerationForOthers(int seed, int gridSize)
    {
        if (seedChosen)
        {
            return;
        }

        //Debug.Log("Seed: " + seed + " scene: " + SceneManager.GetActiveScene().name);

        dungeonGrid.GridSize = new Vector2Int(gridSize, gridSize);

        dungeonGrid.GenerateGrid();

        Random.InitState(seed);
        seedChosen = true;

        StartGeneration();
    }

    private void StartGeneration()
    {
        foreach (GenerationRoomData room in rooms)
        {
            RoomManager roomManager = room.GetComponent<RoomManager>();

            if (roomManager != null && roomManager.RoomMode == RoomManager.RoomModes.BATTLEONLY)
            {
                roomManager.SetRoomMode(roomModesChances[Random.Range(0, roomModesChances.Length)]);
            }            

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

            Vector3 elevatorPos = randomChosenEndingRoom.ChosenEndingOpening.GetChild(0).GetChild(0).position;

            foreach (Transform cell in room.CellsToMakeRoomForOpening)
            {
                Vector3 pos = cell.transform.position;
                DungeonCell val = null;

                if (Vector3.Distance(elevatorPos, pos) > distanceBetweenElevator && dungeonGrid.Grid.TryGetValue(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)), out val))
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

            DungeonCell startingCell = ChooseDoor(roomsDictionary[pos], targetPos);
            DungeonCell endCell = ChooseDoor(roomsDictionary[targetPos], targetPos);

            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                continue;
            }

            List<GridNode> gridNodes = aStarPathFinding.FindPath(startingCell, endCell, currentWallToRemove);

            if (gridNodes != null)
            {
                allHallwayNodes.AddRange(gridNodes);

                foreach (GridNode node in gridNodes)
                {
                    node.dungeonCell.cellType = DungeonCell.CellTypes.HALLWAY;
                }
            }
        }

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Requesting hallways");
            photonView.RPC("RequestHallwayData", RpcTarget.MasterClient, (byte)PhotonNetwork.LocalPlayer.ActorNumber);
            return;
        }

        CreateHallways(allHallwayNodes);
        OnGenerationDone?.Invoke();

        PlayersStatsHolder.instance.CreateSavedStatsList();
    }

    [PunRPC]
    void RequestHallwayData(byte playerIndex)
    {
        playersToSendDataToHallway.Add(PhotonNetwork.PlayerList[playerIndex - 1]);

        if (hallwaysMade)
        {
            SendHallwayDataToOthers();
        }
        else
        {
            OnHallwaysMade += SendHallwayDataToOthers;
        }
    }

    private void SendHallwayDataToOthers()
    {
        foreach (Player player in playersToSendDataToHallway)
        {
            Debug.Log("Sending hallways");
            photonView.RPC("CreateHallwayOthers", player, tilesToSend.ToArray());
        }

        playersToSendDataToHallway.Clear();
    }

    [PunRPC]
    void CreateHallwayOthers(Vector2[] hallwayPos)
    {
        List<HallwayTile> tilesToCheck = new List<HallwayTile>();

        foreach (Vector2 pos in hallwayPos)
        {
            Debug.Log("Placing hallways");
            GameObject hallway = Instantiate(hallwayPrefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
            HallwayTile tile = hallway.GetComponent<HallwayTile>();
            tile.CheckSurroundings(dungeonGrid, this, true);
            tilesToCheck.Add(tile);
        }

        foreach (GenerationRoomData generationRoomData in rooms)
        {
            List<Transform> chosenOpenings = generationRoomData.ChosenOpenings.Distinct().ToList();

            for (int i = 0; i < generationRoomData.Openings.Length; i++)
            {
                Transform opening = generationRoomData.Openings[i];

                if (!generationRoomData.ChosenOpenings.Contains(opening))
                {
                    continue;
                }

                for (int j = 0; j < opening.childCount; j++)
                {
                    Vector3 openingPos = opening.GetChild(j).transform.position;

                    if (placedHallwaysPos.Contains(openingPos))
                    {
                        continue;
                    }

                    GameObject hallway = Instantiate(hallwayPrefab, openingPos, Quaternion.identity);
                    HallwayTile tile = hallway.GetComponent<HallwayTile>();
                    tile.CheckSurroundings(dungeonGrid, this, true);
                    tilesToCheck.Add(tile);
                    placedHallwaysPos.Add(openingPos);

                    DungeonCell cell;

                    if (dungeonGrid.Grid.TryGetValue(new Vector2Int(Mathf.RoundToInt(openingPos.x), Mathf.RoundToInt(openingPos.z)), out cell))
                    {
                        cell.cellType = DungeonCell.CellTypes.HALLWAY;
                        cell.extraWallRemoval = generationRoomData.WallToRemove[i];
                        tile.RemoveWall(cell.extraWallRemoval);
                    }
                }
            }
        }

        foreach (HallwayTile tile in tilesToCheck)
        {
            tile.CheckSurroundings(dungeonGrid, this, true);
        }

        foreach (var item in dungeonGrid.Grid)
        {
            Destroy(item.Value.gameObject);
        }

        dungeonGrid.Grid.Clear();

        OnGenerationDone?.Invoke();
    }

    private void CreateHallways(List<GridNode> allHallwayNodes)
    {
        allHallwayNodes = allHallwayNodes.Distinct().ToList();

        List<HallwayTile> tilesToRecheck = new List<HallwayTile>();
        tilesToSend = new List<Vector2>();

        foreach (GridNode node in allHallwayNodes)
        {
            if (placedHallwaysPos.Contains(node.dungeonCell.transform.position) || node.dungeonCell.extraWallRemoval >= 0)
            {
                continue;
            }

            GameObject hallway = Instantiate(hallwayPrefab, node.dungeonCell.transform.position, Quaternion.identity);
            HallwayTile tile = hallway.GetComponent<HallwayTile>();
            tile.CheckSurroundings(dungeonGrid, this, false);

            tilesToRecheck.Add(tile);
            tilesToSend.Add(new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.z)));
            placedHallwaysPos.Add(node.dungeonCell.transform.position);
        }

        foreach (DungeonCell cell in extraHallways)
        {
            if (placedHallwaysPos.Contains(cell.transform.position) || cell.extraWallRemoval >= 0)
            {
                continue;
            }

            GameObject hallway = Instantiate(hallwayPrefab, cell.transform.position, Quaternion.identity);
            HallwayTile tile = hallway.GetComponent<HallwayTile>();
            tile.CheckSurroundings(dungeonGrid, this, true);
            placedHallwaysPos.Add(cell.transform.position);
            tilesToRecheck.Add(tile);
            tilesToSend.Add(new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.z)));
        }

        foreach (GenerationRoomData generationRoomData in rooms)
        {
            List<Transform> chosenOpenings = generationRoomData.ChosenOpenings.Distinct().ToList();

            for (int i = 0; i < generationRoomData.Openings.Length; i++)
            {
                Transform opening = generationRoomData.Openings[i];

                if (!generationRoomData.ChosenOpenings.Contains(opening))
                {
                    continue;
                }

                for (int j = 0; j < opening.childCount; j++)
                {
                    Vector3 openingPos = opening.GetChild(j).transform.position;

                    if (placedHallwaysPos.Contains(openingPos))
                    {
                        continue;
                    }

                    GameObject hallway = Instantiate(hallwayPrefab, openingPos, Quaternion.identity);
                    HallwayTile tile = hallway.GetComponent<HallwayTile>();
                    tile.CheckSurroundings(dungeonGrid, this, true);
                    tilesToRecheck.Add(tile);
                    placedHallwaysPos.Add(openingPos);

                    DungeonCell cell;

                    if (dungeonGrid.Grid.TryGetValue(new Vector2Int(Mathf.RoundToInt(openingPos.x), Mathf.RoundToInt(openingPos.z)), out cell))
                    {
                        cell.cellType = DungeonCell.CellTypes.HALLWAY;
                        cell.extraWallRemoval = generationRoomData.WallToRemove[i];
                        tile.RemoveWall(cell.extraWallRemoval);
                    }
                }
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

        hallwaysMade = true;
        OnHallwaysMade?.Invoke();
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

            if (distance < lowestDistance && generationRoomData.Openings[i] != generationRoomData.ChosenEndingOpening)
            {
                lowestDistance = distance;
                chosenDoor = generationRoomData.Openings[i].GetChild(0);
                chosenIndex = i;
            }
        }

        if (chosenIndex < 0)
        {
            return null;
        }

        if (generationRoomData.WallToRemove.Length > 0)
        {
            currentWallToRemove = generationRoomData.WallToRemove[chosenIndex];
        }        

        Vector2Int flooredPos = new Vector2Int(Mathf.FloorToInt(chosenDoor.position.x), Mathf.FloorToInt(chosenDoor.position.z));
        generationRoomData.ChosenOpenings.Add(generationRoomData.Openings[chosenIndex]);

        DungeonCell cell;

        if (dungeonGrid.Grid.TryGetValue(flooredPos, out cell))
        {
            cell.extraWallRemoval = currentWallToRemove;

            return cell;
        }
        else
        {
            Vector2Int ceiledPos = new Vector2Int(Mathf.CeilToInt(chosenDoor.position.x), Mathf.CeilToInt(chosenDoor.position.z));

            if (!dungeonGrid.Grid.TryGetValue(ceiledPos, out cell))
            {
                float closestDist = Mathf.Infinity;

                foreach (var item in dungeonGrid.Grid)
                {
                    float dist = Vector2Int.Distance(ceiledPos, item.Key);

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        cell = item.Value;
                    }
                }
            }

            cell.extraWallRemoval = currentWallToRemove;

            return cell;
        }
    }

    private void PlaceRoom()
    {
        int randRoom = Random.Range(0, chosenRoomPrefabs.Count);
        GameObject roomPrefab = chosenRoomPrefabs[randRoom];
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

        GameObject room = PhotonFunctionHandler.InstantiateGameObject(roomPrefab, pos, Quaternion.identity);
        GenerationRoomData generationRoomData = room.GetComponent<GenerationRoomData>();

        if (room.CompareTag("StartingRoom"))
        {
            StartingRoom = generationRoomData;
        }

        rooms.Add(generationRoomData);

        chosenRoomPrefabs.RemoveAt(randRoom);
    }

    [PunRPC]
    void PlaceRoomOthers(int viewId, Vector2 pos)
    {
        if (seedChosen)
        {
            return;
        }

        Debug.Log("Room " + viewId + " placed!");

        GameObject room = PhotonNetwork.GetPhotonView(viewId).gameObject;
        room.transform.position = new Vector3(pos.x, 0, pos.y);
        GenerationRoomData generationRoomData = room.GetComponent<GenerationRoomData>();

        if (room.CompareTag("StartingRoom"))
        {
            StartingRoom = generationRoomData;
        }

        rooms.Add(generationRoomData);
    }

    private void PlaceEnd()
    {
        List<GenerationRoomData> roomsThatCanHaveEnds = new List<GenerationRoomData>();

        foreach (GenerationRoomData room in rooms)
        {
            RoomManager roomManager = room.GetComponent<RoomManager>();

            if (roomManager != null && roomManager.RoomMode != RoomManager.RoomModes.NONE)
            {
                roomsThatCanHaveEnds.Add(room);
            }
        }

        int rand = Random.Range(0, roomsThatCanHaveEnds.Count);
        GenerationRoomData chosenRoom = roomsThatCanHaveEnds[rand];

        int randOpening = Random.Range(0, chosenRoom.ChosenOpenings.Count);
        chosenRoom.ChosenEndingOpening = chosenRoom.EndOpenings[randOpening];

        chosenRoom.ChosenEndingOpening.GetChild(0).GetChild(0).gameObject.SetActive(true);

        float extraScale = 0.4f;

        if (chosenRoom.RoomScaleObject.localScale.y > 1)
        {
            extraScale = 0;
        }

        chosenRoom.RoomScaleObject.localScale += new Vector3(extraScale, 0, 0.4f);

        randomChosenEndingRoom = chosenRoom;
        randomChosenOpeningIndex = randOpening;
    }

    [PunRPC]
    void PlaceEndOthers(int roomIndex, int openingIndex)
    {
        if (seedChosen)
        {
            return;
        }

        Debug.Log("End placed at " + roomIndex + " and opening chosen was: " + openingIndex);
        GenerationRoomData chosenRoom = rooms[roomIndex];
        chosenRoom.ChosenEndingOpening = chosenRoom.EndOpenings[openingIndex];

        chosenRoom.ChosenEndingOpening.GetChild(0).GetChild(0).gameObject.SetActive(true);
        chosenRoom.RoomScaleObject.localScale += new Vector3(0.4f, 0, 0.4f);

        randomChosenEndingRoom = chosenRoom;
        randomChosenOpeningIndex = openingIndex;
    }

    private void SeperateRooms()
    {
        int loopCounter = 0, maxLoops = 20000; //just an example

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

                        room.transform.position += dirToOtherRoom.normalized * seperationMultiplier;

                        float clampedX = Mathf.Clamp(room.transform.position.x, room.RoomScaleObject.localScale.x * padding,
                            dungeonGrid.GridSize.x - room.RoomScaleObject.localScale.x * padding);

                        float clampedZ = Mathf.Clamp(room.transform.position.z, room.RoomScaleObject.localScale.z * padding,
                            dungeonGrid.GridSize.y - room.RoomScaleObject.localScale.z * padding);

                        //if (room.oldPos == room.transform.position)
                        //{
                        //    room.StuckTimes++;

                        //    if (room.StuckTimes > 2)
                        //    {
                        //        int roundedX = Mathf.RoundToInt(room.RoomScaleObject.localScale.x * padding);
                        //        int roundedZ = Mathf.RoundToInt(room.RoomScaleObject.localScale.z * padding);

                        //        int randX = Random.Range(roundedX, dungeonGrid.GridSize.x - roundedX);
                        //        int randZ = Random.Range(roundedZ, dungeonGrid.GridSize.y - roundedZ);

                        //        room.transform.position = new Vector3(randX, 0, randZ);
                        //        room.oldPos = room.transform.position;

                        //        room.StuckTimes = 0;
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    room.StuckTimes = 0;
                        //}

                        room.transform.position = new Vector3(clampedX, 0, clampedZ);

                        //room.oldPos = room.transform.position;
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
