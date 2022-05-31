using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    private PlayerUI playerUI;

    [SerializeField] private int difficultyLevel = 1;
    public int DifficultyLevel { get { return difficultyLevel; } }
    public static DifficultyManager instance;

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

    public void IncreaseDifficulty()
    {
        difficultyLevel++;
    }
}
