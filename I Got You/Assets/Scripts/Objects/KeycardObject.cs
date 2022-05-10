using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeycardObject : InteractableObject
{
    [SerializeField] private PlayerStats.ClassNames assignedClass;

    public PlayerStats.ClassNames AssignedClass { get { return assignedClass; } set { assignedClass = value; } }

    protected override void PlayerTriggerEntered(PlayerStats playerStats)
    {
        base.PlayerTriggerEntered(playerStats);

        if (playerStats.CurrentClass == assignedClass)
        {
            playerStats.OnInteract += (PlayerStats stats) => { stats.PickUpInteractable(this); gameObject.SetActive(false); };
        }
        else
        {
            Debug.Log("Not autorized to hold Keycard!");
        }
    }
}
