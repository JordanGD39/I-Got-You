using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPun
{
    private EnemySpawnBoxHolder boxHolder;
    private DifficultyManager difficultyManager;
    private EnemyGenerator enemyGenerator;

    [SerializeField] private List<EnemyGenerator.GeneratedEnemyInfo> enemiesInRoom;
    [SerializeField] private List<int> enemiesNotPlacedCount = new List<int>();
    [SerializeField] private float enemyPlaceAtY = 0;
    [SerializeField] private int limitEnemyCount = 10;
    [SerializeField] private int enemyDeathsInRoom = 0;
    [SerializeField] private int enemyDeathsToClearRoom = 0;
    [SerializeField] private int healthIncreasePerLevel = 20;
    [SerializeField] private DoorOpen doorToThisRoom;
    [SerializeField] private DoorOpen doorToOtherRoom;
    private int currentNotPlacedIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        difficultyManager = FindObjectOfType<DifficultyManager>();
        doorToOtherRoom.OnOpenedDoor += PlaceDoorToThisRoom;

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        boxHolder = GetComponentInChildren<EnemySpawnBoxHolder>();
        enemyGenerator = FindObjectOfType<EnemyGenerator>();
        doorToThisRoom.OnOpenedDoor += PlaceEnemies;
    }

    private void PlaceEnemies()
    {
        if (enemiesInRoom != null && enemiesInRoom.Count > 0)
        {
            return;
        }

        int enemiesPlaced = 0;
        int enemyTypesPlaced = 0;

        enemyDeathsInRoom = 0;
        currentNotPlacedIndex = 0;
        enemyDeathsToClearRoom = 0;
        enemiesNotPlacedCount.Clear();
        enemiesInRoom = enemyGenerator.GenerateEnemies();

        foreach (EnemyGenerator.GeneratedEnemyInfo enemyInfo in enemiesInRoom)
        {
            enemyDeathsToClearRoom += enemyInfo.enemyCount;

            int countNotPlaceable = enemyDeathsToClearRoom - limitEnemyCount;

            if (countNotPlaceable < 0)
            {
                countNotPlaceable = 0;
            }

            enemiesNotPlacedCount.Add(countNotPlaceable);

            for (int i = 0; i < enemyInfo.enemiesList.Count; i++)
            {
                if (enemiesPlaced > 10)
                {
                    break;
                }

                PlaceEnemy(enemyInfo.enemiesList[i]);

                enemiesPlaced++;
            }

            foreach (GameObject enemy in enemyInfo.enemiesList)
            {
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                enemyStats.ListIndex = enemyTypesPlaced;
                enemyStats.OnEnemyDied = CountEnemyDeath;
                enemyStats.OnEnemyDied += PlaceNotYetSpawnedEnemy;
            }

            enemyTypesPlaced++;
        }
    }

    private void CountEnemyDeath(GameObject enemy, int index)
    {
        enemyDeathsInRoom++;

        if (enemyDeathsInRoom >= enemyDeathsToClearRoom && enemyDeathsInRoom > 0)
        {
            ClearRoom();
            enemyDeathsInRoom = 0;

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("ClearRoomForOthers", RpcTarget.Others);
            }
        }
    }

    [PunRPC]
    void ClearRoomForOthers()
    {
        ClearRoom();
    }

    private void ClearRoom()
    {
        difficultyManager.IncreaseDifficulty();

        doorToOtherRoom.gameObject.SetActive(true);
        doorToOtherRoom.CloseOpeningDoor();
        doorToThisRoom.gameObject.SetActive(false);

        if (enemiesInRoom != null)
        {
            enemiesInRoom.Clear();
        }        
    }

    private void PlaceDoorToThisRoom()
    {
        Invoke(nameof(DelaySwitchDoor), 1);
    }

    private void DelaySwitchDoor()
    {
        doorToOtherRoom.ResetDoor();
        doorToOtherRoom.gameObject.SetActive(false);
        doorToThisRoom.gameObject.SetActive(true);
        doorToThisRoom.ResetDoor();
    }

    private void PlaceNotYetSpawnedEnemy(GameObject enemyDied, int listIndex)
    {
        enemiesInRoom[listIndex].enemiesList.Remove(enemyDied);
        enemiesInRoom[listIndex].deadEnemiesList.Add(enemyDied);
        enemyDied.SetActive(false);

        if (currentNotPlacedIndex > enemiesNotPlacedCount.Count - 1)
        {
            return;
        }

        while (enemiesNotPlacedCount[currentNotPlacedIndex] <= 0)
        {
            currentNotPlacedIndex++;

            if (currentNotPlacedIndex > enemiesNotPlacedCount.Count - 1)
            {
                return;
            }
        }

        GameObject enemy = enemiesInRoom[currentNotPlacedIndex].deadEnemiesList[0];
        PlaceEnemy(enemy);
        enemiesInRoom[currentNotPlacedIndex].deadEnemiesList.RemoveAt(0);
        enemiesNotPlacedCount[currentNotPlacedIndex]--;
    }

    private void PlaceEnemy(GameObject enemy)
    {
        BoxCollider chosenBox = boxHolder.SpawnBoxes[Random.Range(0, boxHolder.SpawnBoxes.Count)];
        Vector3 center = chosenBox.transform.position;
        Vector3 randomPosInBox = center + new Vector3(Random.Range(-chosenBox.size.x, chosenBox.size.x), 0, Random.Range(-chosenBox.size.z, chosenBox.size.z));
        randomPosInBox.y = enemyPlaceAtY;

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("PlaceEnemyForOthersRPC", RpcTarget.Others, enemy.GetComponent<PhotonView>().ViewID);
        }

        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        stats.Health = stats.StartingHealth + (healthIncreasePerLevel * difficultyManager.DifficultyLevel);
        stats.CallSyncHealth();
        stats.OnEnemyDied = PlaceNotYetSpawnedEnemy;
        stats.OnEnemyDied += CountEnemyDeath;

        enemy.transform.position = randomPosInBox;
        enemy.SetActive(true);
    }

    [PunRPC]
    void PlaceEnemyForOthersRPC(int viewId)
    {
        GameObject enemy = PhotonNetwork.GetPhotonView(viewId).gameObject;
        enemy.SetActive(true);
    }
}
