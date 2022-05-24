using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InteractableObject : MonoBehaviourPun
{
    protected PlayerManager playerManager;
    protected PlayerUI playerUI;
    protected string interactText = " to interact";

    // Start is called before the first frame update
    private void Start()
    {
        if (playerManager != null && playerUI != null)
        {
            return;
        }

        playerManager = FindObjectOfType<PlayerManager>();
        playerUI = FindObjectOfType<PlayerUI>(true);

        AfterStart();
    }

    protected virtual void AfterStart()
    {
        //Start override
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

            if (playerStats != playerManager.LocalPlayer)
            {
                return;
            }

            PlayerTriggerEntered(playerStats);
            //playerStats.OnInteract = (PlayerStats stats) => { pickedUp = true; };
        }
    }

    protected virtual void PlayerTriggerEntered(PlayerStats playerStats)
    {
        playerUI.ShowInteractPanel(interactText);

        playerStats.OnInteract = (PlayerStats stats) => {
            string playerName = PhotonNetwork.IsConnected ? stats.photonView.Owner.NickName : "LocalPlayer";
            Debug.Log(playerName + " interacted with " + gameObject.name);
            playerUI.HideInteractPanel();
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
            playerUI.HideInteractPanel();
            PlayerStats playerStats = playerManager.StatsOfAllPlayers[other];

            if (playerStats != playerManager.LocalPlayer)
            {
                return;
            }

            playerStats.OnInteract = null;
        }
    }
}
