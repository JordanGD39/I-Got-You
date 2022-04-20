using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPun
{
    private EnemySpawnBoxHolder boxHolder_EnemySpawnBoxHolder;
    private DifficultyManager difficultyManager_DifficultyManager;
    private EnemyGenerator enemyGenerator_EnemyGenerator;

    [SerializeField] private System.Collections.Generic.List<EnemyGenerator> enemiesInRoom_List_GeneratedEnemyInfo;
    [SerializeField] private List<int> enemiesNotPlacedCount_List_int = new List<int>();
    [SerializeField] private float enemyPlaceAtY_float = 0;
    [SerializeField] private int limitEnemyCount_int = 10;
    [SerializeField] private int enemyDeathsInRoom_int = 0;
    [SerializeField] private int enemyDeathsToClearRoom_int = 0;
    [SerializeField] private int healthIncreasePerLevel_int = 20;
    [SerializeField] private DoorOpen doorToThisRoom_DoorOpen;
    [SerializeField] private DoorOpen doorToOtherRoom_DoorOpen;
    private int currentNotPlacedIndex_int = 0;

    // Start is called before the first frame update
    void Start_void()
    {
        difficultyManager_DifficultyManager = FindObjectOfType<DifficultyManager>();
        doorToOtherRoom_DoorOpen.OnOpenedDoor_OpenedDoor += PlaceDoorToThisRoom_void;

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        boxHolder_EnemySpawnBoxHolder = GetComponentInChildren<EnemySpawnBoxHolder>();
        enemyGenerator_EnemyGenerator = FindObjectOfType<EnemyGenerator>();
        doorToThisRoom_DoorOpen.OnOpenedDoor_OpenedDoor += PlaceEnemies_void;
    }

    private void PlaceEnemies_void()
    {
        if (enemiesInRoom_List_GeneratedEnemyInfo != null && enemiesInRoom_List_GeneratedEnemyInfo.Count > 0)
        {
            return;
        }

        int enemiesPlaced = 0;
        int enemyTypesPlaced = 0;

        enemyDeathsInRoom_int = 0;
        currentNotPlacedIndex_int = 0;
        enemyDeathsToClearRoom_int = 0;
        enemiesNotPlacedCount_List_int.Clear();
        enemiesInRoom_List_GeneratedEnemyInfo = enemyGenerator_EnemyGenerator.GenerateEnemies_List_GeneratedEnemyInfo();

        for (int i = enemiesInRoom_List_GeneratedEnemyInfo.Count - 1; i >= 0; i--)
        {
            Debug.Log(i);

            EnemyGenerator.GeneratedEnemyInfo enemyInfo = enemiesInRoom_List_GeneratedEnemyInfo[i];

            enemyDeathsToClearRoom_int += enemyInfo.enemyCount_int;

            int countNotPlaceable = enemyDeathsToClearRoom_int - limitEnemyCount_int;

            if (countNotPlaceable < 0)
            {
                countNotPlaceable = 0;
            }

            enemiesNotPlacedCount_List_int.Add(countNotPlaceable);

            for (int j = 0; j < enemyInfo.enemyCount_int; j++)
            {
                if (enemiesPlaced >= 10)
                {
                    break;
                }

                PlaceEnemy_void(enemyInfo.enemiesList_List_GameObject[j]);

                enemiesPlaced++;
            }

            foreach (GameObject enemy in enemyInfo.enemiesList_List_GameObject)
            {
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                enemyStats.ListIndex = i;
                enemyStats.OnEnemyDied = CountEnemyDeath_void;
                enemyStats.OnEnemyDied += PlaceNotYetSpawnedEnemy_void;
            }

            foreach (GameObject enemy in enemyInfo.availableEnemiesList_List_GameObject)
            {
                EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
                enemyStats.ListIndex = -1;
            }
        }
    }

    private void CountEnemyDeath_void(GameObject enemy, int index)
    {
        enemyDeathsInRoom_int++;

        if (enemyDeathsInRoom_int >= enemyDeathsToClearRoom_int && enemyDeathsInRoom_int > 0)
        {
            ClearRoom_void();
            enemyDeathsInRoom_int = 0;

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("ClearRoomForOthers", RpcTarget.Others);
            }
        }
    }

    [PunRPC]
    void ClearRoomForOthers_void()
    {
        ClearRoom_void();
    }

    private void ClearRoom_void()
    {
        difficultyManager_DifficultyManager.IncreaseDifficulty();

        doorToOtherRoom_DoorOpen.gameObject.SetActive(true);
        doorToOtherRoom_DoorOpen.CloseOpeningDoor_void();
        doorToThisRoom_DoorOpen.gameObject.SetActive(false);

        if (enemiesInRoom_List_GeneratedEnemyInfo != null)
        {
            enemiesInRoom_List_GeneratedEnemyInfo.Clear();
        }        
    }

    private void PlaceDoorToThisRoom_void()
    {
        Invoke(nameof(DelaySwitchDoor_void), 1);
    }

    private void DelaySwitchDoor_void()
    {
        doorToOtherRoom_DoorOpen.ResetDoor_void();
        doorToOtherRoom_DoorOpen.gameObject.SetActive(false);
        doorToThisRoom_DoorOpen.gameObject.SetActive(true);
        doorToThisRoom_DoorOpen.ResetDoor_void();
    }

    private void PlaceNotYetSpawnedEnemy_void(GameObject enemyDied, int listIndex)
    {
        Debug.Log(listIndex);

        if (listIndex >= 0 && listIndex < enemiesInRoom_List_GeneratedEnemyInfo.Count)
        {
            EnemyGenerator.GeneratedEnemyInfo generatedEnemyInfo = enemiesInRoom_List_GeneratedEnemyInfo[listIndex];

            generatedEnemyInfo.availableEnemiesList_List_GameObject.Add(enemyDied);
            generatedEnemyInfo.enemiesList_List_GameObject.Remove(enemyDied);
        }
       
        enemyDied.SetActive(false);

        if ((currentNotPlacedIndex_int < 0 || currentNotPlacedIndex_int > enemiesNotPlacedCount_List_int.Count - 1) || enemiesInRoom_List_GeneratedEnemyInfo.Count == 0)
        {
            return;
        }

        if (enemiesNotPlacedCount_List_int[currentNotPlacedIndex_int] <= 0 && currentNotPlacedIndex_int >= enemiesNotPlacedCount_List_int.Count - 1)
        {
            return;
        }

        while (enemiesNotPlacedCount_List_int[currentNotPlacedIndex_int] <= 0)
        {
            currentNotPlacedIndex_int++;

            if (currentNotPlacedIndex_int < 0 || currentNotPlacedIndex_int > enemiesNotPlacedCount_List_int.Count - 1)
            {
                Debug.Log("Too far");
                return;
            }
        }

        GameObject enemy = enemiesInRoom_List_GeneratedEnemyInfo[currentNotPlacedIndex_int].availableEnemiesList_List_GameObject[0];
        PlaceEnemy_void(enemy);
        enemiesInRoom_List_GeneratedEnemyInfo[currentNotPlacedIndex_int].enemiesList_List_GameObject.Add(enemy);
        enemiesInRoom_List_GeneratedEnemyInfo[currentNotPlacedIndex_int].availableEnemiesList_List_GameObject.RemoveAt(0);
        enemiesNotPlacedCount_List_int[currentNotPlacedIndex_int]--;
    }

    private void PlaceEnemy_void(GameObject enemy)
    {
        BoxCollider chosenBox = boxHolder_EnemySpawnBoxHolder.SpawnBoxes_List_BoxCollider[Random.Range(0, boxHolder_EnemySpawnBoxHolder.SpawnBoxes_List_BoxCollider.Count)];
        Vector3 center = chosenBox.transform.position;
        Vector3 randomPosInBox = center + new Vector3(Random.Range(-chosenBox.size.x, chosenBox.size.x), 0, Random.Range(-chosenBox.size.z, chosenBox.size.z));
        randomPosInBox.y = enemyPlaceAtY_float;

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("PlaceEnemyForOthersRPC", RpcTarget.Others, enemy.GetComponent<PhotonView>().ViewID);
        }

        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        stats.Health = stats.StartingHealth + (healthIncreasePerLevel_int * difficultyManager_DifficultyManager.DifficultyLevel);

        if (stats.ListIndex < 0)
        {
            stats.ListIndex = currentNotPlacedIndex_int;
        }
        
        stats.CallSyncHealth();
        stats.OnEnemyDied = PlaceNotYetSpawnedEnemy_void;
        stats.OnEnemyDied += CountEnemyDeath_void;

        enemy.transform.position = randomPosInBox;
        enemy.SetActive(true);
    }

    [PunRPC]
    void PlaceEnemyForOthersRPC_void(int viewId)
    {
        GameObject enemy = PhotonNetwork.GetPhotonView(viewId).gameObject;
        enemy.SetActive(true);
    }
}
