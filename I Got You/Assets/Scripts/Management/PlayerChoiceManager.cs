using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChoiceManager : MonoBehaviour
{
    [SerializeField] private Dictionary<int, PlayerStats.ClassNames> chosenClasses = new Dictionary<int, PlayerStats.ClassNames>();
    public Dictionary<int, PlayerStats.ClassNames> ChosenClasses { get { return chosenClasses; } }

    public static PlayerChoiceManager instance; 

    void Awake()
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
}
