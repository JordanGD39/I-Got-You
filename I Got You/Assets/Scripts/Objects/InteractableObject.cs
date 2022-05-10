using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InteractableObject : MonoBehaviourPun
{
    protected PlayerManager playerManager;

    // Start is called before the first frame update
    private void Start()
    {
        if (playerManager != null)
        {
            return;
        }

        playerManager = FindObjectOfType<PlayerManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerManager == null)
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            PlayerStats playerStats = playerManager.StatsOfAllPlayers[other];

            PlayerTriggerEntered(playerStats);
            //playerStats.OnInteract = (PlayerStats stats) => { pickedUp = true; };
        }
    }

    protected virtual void PlayerTriggerEntered(PlayerStats playerStats)
    {
        playerStats.OnInteract = (PlayerStats stats) => {
            string playerName = PhotonNetwork.IsConnected ? stats.photonView.Owner.NickName : "LocalPlayer";
            Debug.Log(playerName + " interacted with " + gameObject.name);
        };
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerManager == null)
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            PlayerStats playerStats = playerManager.StatsOfAllPlayers[other];

            playerStats.OnInteract = null;
        }
    }
}
