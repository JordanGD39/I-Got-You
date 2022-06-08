using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSphere : MonoBehaviour
{
    private PlayerManager playerManager;
    private List<PlayerStats> playersInRange = new List<PlayerStats>();

    public delegate void FoundAllInRange(List<PlayerStats> inRange);
    public FoundAllInRange OnFoundAllInRange;

    [SerializeField] private float checkTime = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }

        playersInRange.Clear();
        Invoke(nameof(DoneChecking), checkTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol") && playerManager.StatsOfAllPlayers[other] != playerManager.LocalPlayer)
        {
            if (playerManager.StatsOfAllPlayers[other].IsDown)
            {
                return;
            }

            playersInRange.Add(playerManager.StatsOfAllPlayers[other]);
        }
    }

    private void DoneChecking()
    {
        OnFoundAllInRange?.Invoke(playersInRange);
        playersInRange.Clear();
    }
}
