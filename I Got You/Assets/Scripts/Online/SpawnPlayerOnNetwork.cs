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

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.OfflineMode || !PhotonNetwork.InRoom)
        {
            playerOffline.SetActive(true);
            return;
        }

        Vector3 randomPos = new Vector3(Random.Range(minX, maxX), 1, 0);
        PhotonNetwork.Instantiate(playerPrefab.name, randomPos, Quaternion.identity);
    }
}
