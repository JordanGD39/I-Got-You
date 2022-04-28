using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutAnalyze : MonoBehaviour
{
    private GameObject analyzeVolume;
    private EnemyManager enemyManager;

    // Start is called before the first frame update
    void Start()
    {
        analyzeVolume = GameObject.FindGameObjectWithTag("ScoutVolume").transform.GetChild(0).gameObject;
        analyzeVolume.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckAbilityToggle();
    }

    public void CheckAbilityToggle()
    {
        if (Input.GetButtonDown("Ability"))
        {
            analyzeVolume.SetActive(!analyzeVolume.activeSelf);
            enemyManager.ScoutAnalyzing = !enemyManager.ScoutAnalyzing;
        }
    }
}
