using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnBoxHolder : MonoBehaviour
{
    public List<BoxCollider> SpawnBoxes_List_BoxCollider { get; private set; } = new List<BoxCollider>();

    // Start is called before the first frame update
    void Start_void()
    {
        SpawnBoxes_List_BoxCollider.AddRange(transform.GetComponentsInChildren<BoxCollider>());
    }
}
