using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<PlayerStats> players = new List<PlayerStats>();
    public List<PlayerStats> Players { get { return players; } }

    [SerializeField] private List<PlayerStats> playersInGame = new List<PlayerStats>();
    public List<PlayerStats> PlayersInGame { get { return playersInGame; } }

    [SerializeField] private List<PlayerStats> deadPlayers = new List<PlayerStats>();
    public List<PlayerStats> DeadPlayers { get { return deadPlayers; } }
    
    public Dictionary<Collider, PlayerStats> StatsOfAllPlayers { get; private set; } = new Dictionary<Collider, PlayerStats>();

    public PlayerStats LocalPlayer { get; set; }

    public void RemoveMissingPlayers()
    {
        players = players.Where(item => item != null).ToList();
        playersInGame = playersInGame.Where(item => item != null).ToList();
    }

    public bool IsEveryoneDead()
    {
        return deadPlayers.Count == playersInGame.Count;
    }
}
