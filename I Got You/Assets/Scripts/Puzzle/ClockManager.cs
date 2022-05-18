using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> clockObject;
    public int FinishedClocks { get; set; } = 0;

    public void CheckAllCompleted()
    {
        if (FinishedClocks >= clockObject.Count)
        {
            Debug.Log("Door opens!");
        }
    }
}
