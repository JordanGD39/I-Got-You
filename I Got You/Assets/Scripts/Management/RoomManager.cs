using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPun
{
    private EnemySpawnBoxHolder boxHolder;
    private DifficultyManager difficultyManager;
    private EnemyGenerator enemyGenerator;

    private List<GameObject> enemiesInRoom;
    private List<GameObject> enemiesNotYetSpawned = new List<GameObject>();
    [SerializeField] private float enemyPlaceAtY = 0;
    [SerializeField] private float limitEnemyCount = 10;
    [SerializeField] private int enemyDeathsInRoom = 0;
    [SerializeField] private int healthIncreasePerLevel = 20;
    [SerializeField] private DoorOpen doorToThisRoom;
    [SerializeField] private DoorOpen doorToOtherRoom;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        difficultyManager = FindObjectOfType<DifficultyManager>();
        boxHolder = GetComponentInChildren<EnemySpawnBoxHolder>();
        enemyGenerator = FindObjectOfType<EnemyGenerator>();
        doorToThisRoom.OnOpenDoor += PlaceEnemies;
        doorToOtherRoom.OnOpenDoor += PlaceDoorToThisRoom;
    }

    private void PlaceEnemies()
    {
        if (enemiesInRoom != null && enemiesInRoom.Count > 0)
        {
            return;
        }

        enemyDeathsInRoom = 0;
        enemiesInRoom = enemyGenerator.GenerateEnemies();

        int enemyCount = 0;
        bool moreEnemiesThenLimit = false;

        for (int i = 0; i < enemiesInRoom.Count; i++)
        {
            PlaceEnemy(enemiesInRoom[i]);

            if (i >= limitEnemyCount)
            {
                enemyCount = i;
                moreEnemiesThenLimit = true;
                break;
            }
        }

        if (moreEnemiesThenLimit)
        {
            enemiesNotYetSpawned.Clear();

            for (int i = enemyCount; i < enemiesInRoom.Count; i++)
            {
                GameObject enemy = enemiesInRoom[i];
                enemiesNotYetSpawned.Add(enemy);
            }
        }
    }

    private void CountEnemyDeath()
    {
        enemyDeathsInRoom++;

        if (enemyDeathsInRoom >= enemiesInRoom.Count)
        {
            ClearRoom();

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

    private void PlaceNotYetSpawnedEnemy()
    {
        if (enemiesNotYetSpawned.Count == 0)
        {
            return;
        }

        GameObject enemy = enemiesNotYetSpawned[0];
        PlaceEnemy(enemy);
        enemiesNotYetSpawned.RemoveAt(0);
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
