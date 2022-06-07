using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveArea : InteractableObject
{
    private PlayerRevive playerRevive;
    private bool reviving = false;
    [SerializeField] private float reviveTime = 7;

    protected override void AfterStart()
    {
        base.AfterStart();

        playerRevive = GetComponent<PlayerRevive>();
        interactText = " to revive Player";
    }

    protected override void PlayerTriggerEntered(PlayerStats playerStats)
    {
        base.PlayerTriggerEntered(playerStats);

        if (reviving)
        {
            return;
        }

        playerStats.OnInteract += StartRevive;
        playerStats.OnInteractHoldStop = StopRevive;
    }

    private void StartRevive(PlayerStats playerStats)
    {
        playerRevive.StopTimer = true;
        reviving = true;
        playerUI.StartReviveTimer(reviveTime);

        Invoke(nameof(ReviveDone), reviveTime);
    }

    private void ReviveDone()
    {
        playerRevive.Revived(true);
    }

    private void StopRevive(PlayerStats playerStats)
    {
        playerRevive.StopTimer = false;
        playerUI.StopReviveTimer();
        CancelInvoke();
    }
}
