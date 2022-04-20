using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayerOnNetwork : MonoBehaviourPun
{
    [SerializeField] private GameObject playerPrefab_GameObject;
    [SerializeField] private GameObject playerOffline_GameObject;
    [SerializeField] private Vector3 minPos_Vector3;
    [SerializeField] private Vector3 maxPos_Vector3;
    private PlayerManager playerManager_PlayerManager;

    // Start is called before the first frame update
    void Start_void()
    {
        if (playerManager_PlayerManager == null)
        {
            playerManager_PlayerManager = GetComponent<PlayerManager>();
        }
        PlayerStats playerStats;

        if (!PhotonFunctionHandler.IsPlayerOnline_bool())
        {
            playerOffline_GameObject.SetActive(true);

            playerStats = playerOffline_GameObject.GetComponent<PlayerStats>();

            playerManager_PlayerManager.Players.Add(playerStats);
            playerManager_PlayerManager.PlayersInGame.Add(playerStats);
            playerManager_PlayerManager.StatsOfAllPlayers_Dictionary.Add(playerOffline_GameObject.GetComponentInChildren<CapsuleCollider>(), playerStats);
            return;
        }

        Vector3 randomPos = new Vector3(Random.Range(minPos_Vector3.x, maxPos_Vector3.x), 1, Random.Range(minPos_Vector3.z, maxPos_Vector3.z));
        GameObject player = PhotonNetwork.Instantiate(playerPrefab_GameObject.name, randomPos, Quaternion.identity);

        playerStats = player.GetComponent<PlayerStats>();

        playerManager_PlayerManager.Players.Add(playerStats);
        playerManager_PlayerManager.PlayersInGame.Add(playerStats);

        Debug.Log(player.GetComponentInChildren<CapsuleCollider>());

        playerManager_PlayerManager.StatsOfAllPlayers_Dictionary.Add(player.GetComponentInChildren<CapsuleCollider>(), playerStats);
        int photonId = player.GetComponent<PhotonView>().ViewID;

        photonView.RPC("AddPlayerToManager", RpcTarget.OthersBuffered, photonId);
    }

    [PunRPC]
    void AddPlayerToManager_void(int photonId)
    {
        if (playerManager_PlayerManager == null)
        {
            playerManager_PlayerManager = GetComponent<PlayerManager>();
        }

        PlayerStats playerStats = PhotonView.Find(photonId).gameObject.GetComponent<PlayerStats>();

        playerManager_PlayerManager.Players.Add(playerStats);
        playerManager_PlayerManager.PlayersInGame.Add(playerStats);
        playerManager_PlayerManager.StatsOfAllPlayers_Dictionary.Add(playerStats.GetComponentInChildren<CapsuleCollider>(), playerStats);
    }
}
