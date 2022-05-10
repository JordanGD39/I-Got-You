using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KeycardObject : InteractableObject
{
    [SerializeField] private PlayerStats.ClassNames assignedClass;

    public PlayerStats.ClassNames AssignedClass { get { return assignedClass; } set { assignedClass = value; } }

    protected override void PlayerTriggerEntered(PlayerStats playerStats)
    {
        base.PlayerTriggerEntered(playerStats);

        if (playerStats.CurrentClass == assignedClass)
        {
            playerStats.OnInteract += PickUpKeycard;
        }
        else
        {
            Debug.Log("Not autorized to hold Keycard!");
        }
    }

    private void PickUpKeycard(PlayerStats stats)
    {
        stats.PickUpInteractable(this);

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoveKeycardOthers", RpcTarget.Others);
        }

        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void RemoveKeycardOthers()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
