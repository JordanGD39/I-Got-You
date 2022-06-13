using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayersStatsHolder : MonoBehaviour
{
    public static PlayersStatsHolder instance;

    [SerializeField] private SavedPlayerStats[] savedPlayerStats;
    public SavedPlayerStats[] PlayerStatsSaved { get { return savedPlayerStats; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void CreateSavedStatsList()
    {
        savedPlayerStats = new SavedPlayerStats[PhotonNetwork.IsConnected ? PhotonNetwork.CurrentRoom.PlayerCount : 1];
    }

    public void ClearSavedStats()
    {
        savedPlayerStats = new SavedPlayerStats[0];
    }
}

[System.Serializable]
public class SavedPlayerStats
{
    public GunObject[] guns = new GunObject[2];
    public int[] ammo = new int[2];
    public int health = 100;
    public float abilityCharge = 0;
}
