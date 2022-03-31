using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPun
{
    private EnemySpawnBoxHolder boxHolder;
    private EnemyGenerator enemyGenerator;
    private List<GameObject> enemiesInRoom;
    [SerializeField] private float enemyPlaceAtY = 0;

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
        enemiesInRoom = enemyGenerator.GenerateEnemies();

        foreach (GameObject enemy in enemiesInRoom)
        {
            BoxCollider chosenBox = boxHolder.SpawnBoxes[Random.Range(0, boxHolder.SpawnBoxes.Count)];
            Vector3 center = chosenBox.transform.position;
            Vector3 randomPosInBox = center + new Vector3(Random.Range(-chosenBox.size.x, chosenBox.size.x), 0, Random.Range(-chosenBox.size.z, chosenBox.size.z));
            randomPosInBox.y = enemyPlaceAtY;

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("PlaceEnemyForOthersRPC", RpcTarget.Others, enemy.GetComponent<PhotonView>().ViewID);
            }            

            enemy.transform.position = randomPosInBox;
            enemy.SetActive(true);
        }
    }

    [PunRPC]
    void PlaceEnemyForOthersRPC(int viewId)
    {
        GameObject enemy = PhotonNetwork.GetPhotonView(viewId).gameObject;
        enemy.SetActive(true);
    }
}
