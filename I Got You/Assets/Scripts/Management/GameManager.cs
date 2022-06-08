using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private int floor = 0;
    public int Floor { get { return floor; } }
    public int TotalArenaRooms { get; set; } = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void AddFloorLevel()
    {
        floor++;

        DifficultyManager difficultyManager = DifficultyManager.instance;

        if (difficultyManager.DifficultyLevel < Floor * TotalArenaRooms)
        {
            difficultyManager.DifficultyLevel = Floor * TotalArenaRooms;
        }
    }

    public void ResetFloorLevel()
    {
        floor = 0;
    }
}
