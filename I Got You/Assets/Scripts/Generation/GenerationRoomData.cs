using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationRoomData : MonoBehaviour
{
    [SerializeField] private Transform roomScaleObject;
    public Transform RoomScaleObject { get { return roomScaleObject; } }

    [SerializeField] private Transform[] openings;
    public Transform[] Openings { get { return openings; } }

    [SerializeField] private Transform[] cellsToMakeRoomForOpening;
    public Transform[] CellsToMakeRoomForOpening { get { return cellsToMakeRoomForOpening; } }

    [SerializeField] private int[] wallToRemove;
    public int[] WallToRemove { get { return wallToRemove; } }

    [SerializeField] private List<Transform> chosenOpenings = new List<Transform>();
    public List<Transform> ChosenOpenings { get { return chosenOpenings; } }

    public Vector3 oldPos { get; set; } = Vector3.zero;
    public int StuckTimes { get; set; } = 0;
}
