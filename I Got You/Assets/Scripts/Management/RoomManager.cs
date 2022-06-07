using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPun
{
    private EnemySpawnBoxHolder boxHolder;
    private DifficultyManager difficultyManager;
    private EnemyGenerator enemyGenerator;

    public enum RoomModes {NONE, BATTLEONLY, PUZZLEEAT, PUZZLECLOCK, PUZZLESIMON}
    [SerializeField] private RoomModes roomMode;
    public RoomModes RoomMode { get { return roomMode; } }
    [SerializeField] private GameObject[] puzzleObjects;
    [SerializeField] private List<EnemyGenerator.GeneratedEnemyInfo> enemiesInRoom;
    [SerializeField] private List<int> enemiesNotPlacedCount = new List<int>();
    [SerializeField] private float enemyPlaceAtY = 0;
    [SerializeField] private int limitEnemyCount = 10;
    [SerializeField] private int enemyDeathsInRoom = 0;
    [SerializeField] private int enemyDeathsToClearRoom = 0;
    [SerializeField] private int healthIncreasePerLevel = 20;
    [SerializeField] private DoorOpen[] doorsToThisRoom;
    [SerializeField] private DoorOpen[] doorsToOtherRoom;
    [SerializeField] private float puzzleEnemyCountMultiplier = 0.8f;
    private int currentNotPlacedIndex = 0;

    private bool puzzlesCompleted = false;
    private bool battleStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        difficultyManager = FindObjectOfType<DifficultyManager>();

        if (roomMode != RoomModes.PUZZLEEAT)
        {
            for (int i = 0; i < doorsToOtherRoom.Length; i++)
            {
                doorsToOtherRoom[i].OnOpenedDoor += PlaceDoorToThisRoom;
            }
        }

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        boxHolder = GetComponentInChildren<EnemySpawnBoxHolder>();
        enemyGenerator = FindObjectOfType<EnemyGenerator>();
    }

    public void SetRoomMode(RoomModes aRoomMode)
    {
        foreach (GameObject puzzle in puzzleObjects)
        {
            puzzle.SetActive(false);
        }

        roomMode = aRoomMode;

        if (roomMode != RoomModes.PUZZLEEAT && (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient))
        {
            for (int i = 0; i < doorsToThisRoom.Length; i++)
            {
                doorsToThisRoom[i].OnOpenedDoor += PlaceEnemies;
            }
        }

        if (roomMode == RoomModes.BATTLEONLY)
        {
            puzzlesCompleted = true;
        }
        else if (roomMode != RoomModes.NONE && roomMode != RoomModes.PUZZLEEAT)
        {
            puzzleObjects[(int)roomMode - 3].SetActive(true);
            puzzlesCompleted = false;

            if (roomMode == RoomModes.PUZZLESIMON)
            {
                for (int i = 0; i < doorsToThisRoom.Length; i++)
                {
                    doorsToThisRoom[i].OnOpenedDoor += GetComponentInChildren<PuzzleManager>().RemoveScreensWhenEnteredRoom;
                }
            }
        }
    }

    private void PlaceEnemies()
    {
        if (enemiesInRoom != null && enemiesInRoom.Count > 0 || battleStarted)
        {
            return;
        }

        int enemiesPlaced = 0;
        battleStarted = true;

        enemyDeathsInRoom = 0;
        currentNotPlacedIndex = 0;
        enemyDeathsToClearRoom = 0;
        enemiesNotPlacedCount.Clear();
        enemiesInRoom = enemyGenerator.GenerateEnemies(roomMode == RoomModes.BATTLEONLY ? 1 : puzzleEnemyCountMultiplier);

        for (int i = enemiesInRoom.Count - 1; i >= 0; i--)
        {
            Debug.Log(i);

            EnemyGenerator.GeneratedEnemyInfo enemyInfo = enemiesInRoom[i];

            enemyDeathsToClearRoom += enemyInfo.enemyCount;

            int countNotPlaceable = enemyDeathsToClearRoom - limitEnemyCount;

            if (countNotPlaceable < 0)
            {
                countNotPlaceable = 0;
            }

            enemiesNotPlacedCount.Add(countNotPlaceable);

            for (int j = 0; j < enemyInfo.enemyCount; j++)
            {
                if (enemiesPlaced >= 10)
                {
                    break;
                }

                PlaceEnemy(enemyInfo.enemiesList[j]);

                enemiesPlaced++;
            }

            foreach (GameObject enemy in enemyInfo.enemiesList)
            {
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                enemyStats.ListIndex = i;

                enemyStats.OnEnemyDied = PlaceNotYetSpawnedEnemy;

                if (roomMode == RoomModes.BATTLEONLY)
                {
                    enemyStats.OnEnemyDied += CountEnemyDeath;
                }               
            }

            foreach (GameObject enemy in enemyInfo.availableEnemiesList)
            {
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                enemyStats.ListIndex = -1;
            }
        }
    }

    private void CountEnemyDeath(GameObject enemy, int index)
    {
        enemyDeathsInRoom++;

        if (enemyDeathsInRoom >= enemyDeathsToClearRoom && enemyDeathsInRoom > 0)
        {
            ClearRoom();
            enemyDeathsInRoom = 0;

            if (PhotonNetwork.IsConnected && puzzlesCompleted)
            {
                photonView.RPC("ClearRoomForOthers", RpcTarget.Others);
            }
        }
    }

    [PunRPC]
    void ClearRoomForOthers()
    {
        puzzlesCompleted = true;

        ClearRoom();
    }

    private void ClearRoom()
    {
        Debug.Log("Clearing room! " + puzzlesCompleted);

        if (!puzzlesCompleted)
        {
            return;
        }

        difficultyManager.IncreaseDifficulty();
        
        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            OpenAllDoors();
        }

        if (enemiesInRoom != null)
        {
            enemiesInRoom.Clear();
        }        
    }

    public void OpenAllDoors()
    {
        puzzlesCompleted = true;

        foreach (DoorOpen door in doorsToOtherRoom)
        {
            door.CloseOpeningDoor();
        }

        foreach (DoorOpen door in doorsToThisRoom)
        {
            door.OpenOnly = true;
            door.OpenDoor();
            door.OpenClosedDoor();
        }
    }

    private void PlaceDoorToThisRoom()
    {
        Invoke(nameof(DelaySwitchDoor), 1);
    }

    private void DelaySwitchDoor()
    {
        foreach (DoorOpen door in doorsToOtherRoom)
        {
            door.ResetDoor();
            door.gameObject.SetActive(false);
        }

        foreach (DoorOpen door in doorsToThisRoom)
        {
            door.gameObject.SetActive(true);
            door.ResetDoor();
        }
    }

    private void PlaceNotYetSpawnedEnemy(GameObject enemyDied, int listIndex)
    {
        if (listIndex >= 0 && listIndex < enemiesInRoom.Count && enemiesInRoom.Count > 0)
        {
            EnemyGenerator.GeneratedEnemyInfo generatedEnemyInfo = enemiesInRoom[listIndex];

            generatedEnemyInfo.availableEnemiesList.Add(enemyDied);
            generatedEnemyInfo.enemiesList.Remove(enemyDied);
        }

        //enemyDied.SetActive(false);

        if (roomMode != RoomModes.BATTLEONLY && puzzlesCompleted)
        {
            return;
        }

        EnemyGenerator.GeneratedEnemyInfo chosenEnemyType = enemiesInRoom[0];

        if (roomMode == RoomModes.BATTLEONLY)
        {
            if ((currentNotPlacedIndex < 0 || currentNotPlacedIndex > enemiesNotPlacedCount.Count - 1) || enemiesInRoom.Count == 0)
            {
                return;
            }

            if (enemiesNotPlacedCount[currentNotPlacedIndex] <= 0 && currentNotPlacedIndex >= enemiesNotPlacedCount.Count - 1)
            {
                return;
            }

            while (enemiesNotPlacedCount[currentNotPlacedIndex] <= 0)
            {
                currentNotPlacedIndex++;

                if (currentNotPlacedIndex < 0 || currentNotPlacedIndex > enemiesNotPlacedCount.Count - 1)
                {
                    Debug.Log("Too far");
                    return;
                }
            }

            enemiesNotPlacedCount[currentNotPlacedIndex]--;
            chosenEnemyType = enemiesInRoom[currentNotPlacedIndex];
        }
        else
        {
            float rand = Random.value;
            float percent = 0;

            for (int i = 0; i < enemiesInRoom.Count; i++)
            {
                percent += enemiesInRoom[i].spawnPercent;

                if (rand < percent)
                {
                    chosenEnemyType = enemiesInRoom[i];
                    currentNotPlacedIndex = i;
                    break;
                }
            }            
        } 

        GameObject enemy = chosenEnemyType.availableEnemiesList[0];
        PlaceEnemy(enemy);
        enemiesInRoom[currentNotPlacedIndex].enemiesList.Add(enemy);
        enemiesInRoom[currentNotPlacedIndex].availableEnemiesList.RemoveAt(0);
    }

    private void PlaceEnemy(GameObject enemy)
    {
        int rand = Random.Range(0, boxHolder.SpawnBoxes.Count);
        BoxCollider chosenBox = boxHolder.SpawnBoxes[rand];
        Vector3 center = chosenBox.transform.position;
        Vector3 randomPosInBox = center + new Vector3(Random.Range(-chosenBox.size.x, chosenBox.size.x), 0, Random.Range(-chosenBox.size.z, chosenBox.size.z));
        randomPosInBox.y = enemyPlaceAtY;

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("PlaceEnemyForOthersRPC", RpcTarget.Others, enemy.GetComponent<PhotonView>().ViewID);
        }

        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        stats.Health = stats.StartingHealth + (healthIncreasePerLevel * difficultyManager.DifficultyLevel);

        if (stats.ListIndex < 0)
        {
            stats.ListIndex = currentNotPlacedIndex;
        }
        
        stats.CallSyncHealth(0, Vector3.zero);
        stats.CallDisableRagdoll();
        stats.OnEnemyDied = PlaceNotYetSpawnedEnemy;
        stats.OnEnemyDied += CountEnemyDeath;

        enemy.transform.position = randomPosInBox;
        Debug.Log("Enemy with index: " + stats.ListIndex + " spawned in box: " + rand + " pos: " + randomPosInBox);
        enemy.SetActive(true);
    }

    [PunRPC]
    void PlaceEnemyForOthersRPC(int viewId)
    {
        GameObject enemy = PhotonNetwork.GetPhotonView(viewId).gameObject;
        enemy.SetActive(true);
    }
}
