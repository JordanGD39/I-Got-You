using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Dictionary<Transform, EnemyStats> statsOfAllEnemies = new Dictionary<Transform, EnemyStats>();
    public Dictionary<Transform, EnemyStats> StatsOfAllEnemies { get { return statsOfAllEnemies; } }
}
