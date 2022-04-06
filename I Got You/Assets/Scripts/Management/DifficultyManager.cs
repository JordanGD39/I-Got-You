using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    private PlayerUI playerUI;

    [SerializeField] private int difficultyLevel = 1;
    public int DifficultyLevel { get { return difficultyLevel; } }

    private void Start()
    {
        playerUI = FindObjectOfType<PlayerUI>();
        playerUI.UpdateRoundText(difficultyLevel);
    }

    public void IncreaseDifficulty()
    {
        difficultyLevel++;
        playerUI.UpdateRoundText(difficultyLevel);
    }
}
