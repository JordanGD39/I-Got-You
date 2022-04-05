using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnBoxHolder : MonoBehaviour
{
    public List<BoxCollider> SpawnBoxes { get; private set; } = new List<BoxCollider>();

    // Start is called before the first frame update
    void Start()
    {
        SpawnBoxes.AddRange(transform.GetComponentsInChildren<BoxCollider>());
    }
}
