using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayerOnNetwork : MonoBehaviourPun
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerOffline;
    [SerializeField] private Vector3 minPos;
    [SerializeField] private Vector3 maxPos;
    private PlayerManager playerManager;
    private DungeonGenerator dungeonGenerator;

    // Start is called before the first frame update
    void Start()
    {
        if (playerManager == null)
        {
            playerManager = GetComponent<PlayerManager>();
        }

        dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGenerator.StartingRoom == null)
        {
            if (PhotonNetwork.IsConnected)
            {
                dungeonGenerator.OnGenerationDone += PlaceOnlinePlayer;
            }
            else
            {
                dungeonGenerator.OnGenerationDone += PlaceOfflinePlayer;
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                PlaceOnlinePlayer();
            }
            else
            {
                PlaceOfflinePlayer();
            }
        }
    }

    [PunRPC]
    void AddPlayerToManager(int photonId)
    {
        if (playerManager == null)
        {
            playerManager = GetComponent<PlayerManager>();
        }

        PlayerStats playerStats = PhotonView.Find(photonId).gameObject.GetComponent<PlayerStats>();

        playerManager.Players.Add(playerStats);
        playerManager.PlayersInGame.Add(playerStats);
        playerManager.StatsOfAllPlayers.Add(playerStats.GetComponentInChildren<CapsuleCollider>(), playerStats);
    }

    private void PlaceOfflinePlayer()
    {
        playerOffline.transform.position = dungeonGenerator.StartingRoom.transform.position + Vector3.up * 1;

        playerOffline.SetActive(true);

        PlayerStats playerStats = playerOffline.GetComponent<PlayerStats>();
        playerManager.LocalPlayer = playerStats;

        playerManager.Players.Add(playerStats);
        playerManager.PlayersInGame.Add(playerStats);
        playerManager.StatsOfAllPlayers.Add(playerOffline.GetComponentInChildren<CapsuleCollider>(), playerStats);
    }

    private void PlaceOnlinePlayer()
    {
        Vector3 startingRoomPos = dungeonGenerator.StartingRoom.transform.position;
        Vector3 randomPos = new Vector3(Random.Range(minPos.x, maxPos.x), 1, Random.Range(minPos.z, maxPos.z));
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, startingRoomPos + randomPos, Quaternion.identity);

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        playerManager.LocalPlayer = playerStats;

        playerManager.Players.Add(playerStats);
        playerManager.PlayersInGame.Add(playerStats);

        playerManager.StatsOfAllPlayers.Add(player.GetComponentInChildren<CapsuleCollider>(), playerStats);
        int photonId = player.GetComponent<PhotonView>().ViewID;

        photonView.RPC("AddPlayerToManager", RpcTarget.OthersBuffered, photonId);
    }
}
