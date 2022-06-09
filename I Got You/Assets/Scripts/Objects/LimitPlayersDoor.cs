using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LimitPlayersDoor : MonoBehaviour
{
    public List<GameObject> PlayersInRange { get; private set; } = new List<GameObject>();
    [SerializeField] private int limitedPlayerCount = 1;
    private PlayerManager playerManager;
    private PlayerUI playerUI;

    public bool TooManyPlayers { get; private set; } = false;

    private void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        playerUI = FindObjectOfType<PlayerUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other].IsDead)
            {
                return;
            }

            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.ShowNotification("Only " + limitedPlayerCount + " person may enter!");
            }

            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                return;
            }

            playerManager.RemoveMissingPlayers();

            PlayersInRange.Add(other.gameObject);

            TooManyPlayers = PlayersInRange.Count > limitedPlayerCount;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.HideInteractPanel();
            }

            if (!PlayersInRange.Contains(other.gameObject))
            {
                return;
            }

            PlayersInRange.Remove(other.gameObject);

            TooManyPlayers = PlayersInRange.Count > limitedPlayerCount;
        }
    }
}
