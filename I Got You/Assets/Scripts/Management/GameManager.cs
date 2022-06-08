using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int Floor { get; private set; } = 0;
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
        Floor++;

        DifficultyManager difficultyManager = DifficultyManager.instance;

        if (difficultyManager.DifficultyLevel < Floor * TotalArenaRooms)
        {
            difficultyManager.DifficultyLevel = Floor * TotalArenaRooms;
        }
    }

    public void ResetFloorLevel()
    {
        Floor = 0;
    }
}
