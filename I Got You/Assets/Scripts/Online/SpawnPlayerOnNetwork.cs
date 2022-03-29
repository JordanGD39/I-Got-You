using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayerOnNetwork : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerOffline;
    [SerializeField] private float minX = 0;
    [SerializeField] private float maxX = 0;
    private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        if (playerManager == null)
        {
            playerManager = GetComponent<PlayerManager>();
        }

        if (PhotonNetwork.OfflineMode || !PhotonNetwork.InRoom)
        {
            playerOffline.SetActive(true);
            playerManager.Players.Add(playerOffline.GetComponent<PlayerStats>());
            return;
        }

        Vector3 randomPos = new Vector3(Random.Range(minX, maxX), 1, 0);
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, randomPos, Quaternion.identity);

        PlayerStats playerStats = player.GetComponent<PlayerStats>();

        playerManager.Players.Add(playerStats);
        int photonId = player.GetComponent<PhotonView>().ViewID;

        GetComponent<PhotonView>().RPC("AddPlayerToManager", RpcTarget.OthersBuffered, photonId);
    }

    [PunRPC]
    void AddPlayerToManager(int photonId)
    {
        if (playerManager == null)
        {
            playerManager = GetComponent<PlayerManager>();
        }

        playerManager.Players.Add(PhotonView.Find(photonId).gameObject.GetComponent<PlayerStats>());
    }
}
