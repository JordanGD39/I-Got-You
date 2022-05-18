using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KeycardObject : InteractableObject
{
    [SerializeField] private PlayerStats.ClassNames assignedClass;

    public PlayerStats.ClassNames AssignedClass { get { return assignedClass; } set { assignedClass = value; UpdateKeycardModel(); } }

    [SerializeField] private Transform classCardsParent;

    protected override void AfterStart()
    {
        interactText = " to pickup Keycard";
    }

    private void UpdateKeycardModel()
    {
        foreach (Transform child in classCardsParent)
        {
            child.gameObject.SetActive(false);
        }

        classCardsParent.GetChild((int)assignedClass).gameObject.SetActive(true);
    }

    protected override void PlayerTriggerEntered(PlayerStats playerStats)
    {
        base.PlayerTriggerEntered(playerStats);

        if (playerStats.CurrentClass == assignedClass)
        {
            playerStats.OnInteract += PickUpKeycard;
        }
        else
        {
            playerUI.ShowNotification("Not autorized to hold Keycard");
        }
    }

    private void PickUpKeycard(PlayerStats stats)
    {
        stats.PickUpInteractable(this);
        stats.OnInteract = null;

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoveKeycardOthers", RpcTarget.Others);
        }

        transform.GetChild(0).gameObject.SetActive(false);
    }

    [PunRPC]
    void RemoveKeycardOthers()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
