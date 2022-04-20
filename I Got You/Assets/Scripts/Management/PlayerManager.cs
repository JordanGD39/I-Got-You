using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerStats> players_List_PlayerStats;
    private List<PlayerStats> playersInGame_List_PlayerStats;
    private List<PlayerStats> deadPlayers_List_PlayerStats;
    public List<PlayerStats> Players { get { return players_List_PlayerStats; } }
    public List<PlayerStats> PlayersInGame { get { return playersInGame_List_PlayerStats; } }
    public List<PlayerStats> DeadPlayers { get { return deadPlayers_List_PlayerStats; } }
    
    public Dictionary<Collider, PlayerStats> StatsOfAllPlayers_Dictionary { get; private set; } = new Dictionary<Collider, PlayerStats>();
}
