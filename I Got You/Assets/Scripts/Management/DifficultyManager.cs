using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{

    [SerializeField] private int difficultyLevel_int = 1;
    public int DifficultyLevel { get { return difficultyLevel_int; } }

    private void Start()
    {
        playerUI_P = FindObjectOfType<PlayerUI>();
        playerUI_P.UpdateRoundText(difficultyLevel_int);
    }

    public void IncreaseDifficulty()
    {
        difficultyLevel_int++;
        playerUI_P.UpdateRoundText(difficultyLevel_int);
    }
}
