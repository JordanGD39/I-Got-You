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

    // Start is called before the first frame update
    void Start()
    {
        if (playerManager == null)
        {
            playerManager = GetComponent<PlayerManager>();
        }
        PlayerStats playerStats;

        if (!PhotonFunctionHandler.IsPlayerOnline())
        {
            playerOffline.SetActive(true);

            playerStats = playerOffline.GetComponent<PlayerStats>();

            playerManager.Players.Add(playerStats);
            playerManager.PlayersInGame.Add(playerStats);
            playerManager.StatsOfAllPlayers.Add(playerOffline.GetComponentInChildren<CapsuleCollider>(), playerStats);
            return;
        }

        Vector3 randomPos = new Vector3(Random.Range(minPos.x, maxPos.x), 1, Random.Range(minPos.z, maxPos.z));
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, randomPos, Quaternion.identity);

        playerStats = player.GetComponent<PlayerStats>();

        playerManager.Players.Add(playerStats);
        playerManager.PlayersInGame.Add(playerStats);

        Debug.Log(player.GetComponentInChildren<CapsuleCollider>());

        playerManager.StatsOfAllPlayers.Add(player.GetComponentInChildren<CapsuleCollider>(), playerStats);
        int photonId = player.GetComponent<PhotonView>().ViewID;

        photonView.RPC("AddPlayerToManager", RpcTarget.OthersBuffered, photonId);
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
}
