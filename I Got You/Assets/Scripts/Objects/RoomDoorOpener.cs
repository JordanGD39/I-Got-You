using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDoorOpener : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private int puzzlesToComplete = 2;
    private int puzzlesCompleted = 0;

    public void CheckCompletion()
    {
        puzzlesCompleted++;

        if (puzzlesCompleted >= puzzlesToComplete)
        {
            roomManager.OpenAllDoors();
        }
    }
}
