using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Dictionary<Transform, EnemyStats> statsOfAllEnemies = new Dictionary<Transform, EnemyStats>();
    public Dictionary<Transform, EnemyStats> StatsOfAllEnemies { get { return statsOfAllEnemies; } }

    public bool ScoutAnalyzing { get; set; } = false;
    public Transform EnemiesTarget { get; set; } = null;
}
