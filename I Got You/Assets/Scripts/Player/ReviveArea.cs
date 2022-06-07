using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveArea : InteractableObject
{
    private PlayerRevive playerRevive;
    private PlayerStats playerStats;
    private bool reviving = false;
    [SerializeField] private float reviveTime = 7;

    protected override void AfterStart()
    {
        base.AfterStart();

        playerRevive = GetComponent<PlayerRevive>();
        playerStats = GetComponent<PlayerStats>();
        interactText = " to revive Player";
    }

    protected override void PlayerTriggerEntered(PlayerStats stats)
    {
        if (playerStats == stats)
        {
            return;
        }

        base.PlayerTriggerEntered(stats);

        if (reviving)
        {
            return;
        }

        stats.OnInteract += StartRevive;
        stats.OnInteractHoldStop = StopRevive;
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
