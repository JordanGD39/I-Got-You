using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<PlayerStats> players = new List<PlayerStats>();
    public List<PlayerStats> Players { get { return players; } }

    [SerializeField] private List<PlayerStats> deadPlayers = new List<PlayerStats>();
    public List<PlayerStats> DeadPlayers { get { return deadPlayers; } }
    
    public Dictionary<Collider, PlayerStats> StatsOfAllPlayers { get; private set; } = new Dictionary<Collider, PlayerStats>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
