using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveArea : InteractableObject
{
    private PlayerRevive playerRevive;
    private PlayerStats playerStats;
    private PlayerStats currentRevivingPlayer;
    private bool reviving = false;
    [SerializeField] private float reviveTime = 7;

    protected override void AfterStart()
    {
        base.AfterStart();

        playerRevive = GetComponentInParent<PlayerRevive>();
        playerStats = GetComponentInParent<PlayerStats>();
        interactText = " to revive Player";
    }

    protected override void PlayerTriggerEntered(PlayerStats stats)
    {
        if (playerStats == stats || playerStats.IsDead || stats.IsDown)
        {
            return;
        }

        base.PlayerTriggerEntered(stats);
        currentRevivingPlayer = stats;

        if (reviving)
        {
            return;
        }

        stats.OnInteract += StartRevive;
        stats.OnInteractHoldStop = StopRevive;
    }

    private void Update()
    {
        if (playerStats.IsDead && reviving)
        {
            StopRevive(currentRevivingPlayer);
        }
    }

    private void StartRevive(PlayerStats stats)
    {
        playerRevive.SetTimer(true, true);
        reviving = true;
        playerUI.HideInteractPanel();
        playerUI.StartReviveTimer(reviveTime);

        Invoke(nameof(ReviveDone), reviveTime);
    }

    private void ReviveDone()
    {
        playerRevive.Revived(true);
        currentRevivingPlayer.OnInteract = null;
        currentRevivingPlayer.OnInteractHoldStop = null;
        reviving = false;
    }

    private void StopRevive(PlayerStats stats)
    {
        stats.OnInteract = null;
        stats.OnInteractHoldStop = null;
        playerRevive.SetTimer(false, true);
        playerUI.StopReviveTimer();
        reviving = false;
        CancelInvoke();
    }
}
