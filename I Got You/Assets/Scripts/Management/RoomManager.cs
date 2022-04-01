using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPun
{
    private EnemySpawnBoxHolder boxHolder;
    private EnemyGenerator enemyGenerator;
    private List<GameObject> enemiesInRoom;
    private List<GameObject> enemiesNotYetSpawned = new List<GameObject>();
    [SerializeField] private float enemyPlaceAtY = 0;
    [SerializeField] private float limitEnemyCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        boxHolder = GetComponentInChildren<EnemySpawnBoxHolder>();
        enemyGenerator = FindObjectOfType<EnemyGenerator>();
        GetComponentInChildren<DoorOpen>().OnOpenDoor += PlaceEnemies;
    }

    private void PlaceEnemies()
    {
        if (enemiesInRoom != null && enemiesInRoom.Count > 0)
        {
            return;
        }

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
                enemy.GetComponent<EnemyStats>().OnEnemyDied = PlaceNotYetSpawnedEnemy;
                enemiesNotYetSpawned.Add(enemy);
            }
        }
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

        enemy.GetComponent<EnemyStats>().OnEnemyDied = PlaceNotYetSpawnedEnemy;
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
