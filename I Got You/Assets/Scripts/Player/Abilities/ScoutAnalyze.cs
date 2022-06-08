using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScoutAnalyze : MonoBehaviourPun
{
    private GameObject analyzeVolume;
    private EnemyManager enemyManager;
    private PlayerShoot playerShoot;
    private PlayerUI playerUI;

    // Start is called before the first frame update
    void Start()
    {
        analyzeVolume = GameObject.FindGameObjectWithTag("ScoutVolume").transform.GetChild(0).gameObject;
        analyzeVolume.SetActive(false);
        enemyManager = FindObjectOfType<EnemyManager>();
        playerShoot = GetComponent<PlayerShoot>();
        playerUI = FindObjectOfType<PlayerUI>();
        playerUI.UpdateAbility(0, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        CheckAbilityToggle();
    }

    public void CheckAbilityToggle()
    {
        if (Input.GetButtonDown("Ability"))
        {
            analyzeVolume.SetActive(!analyzeVolume.activeSelf);
            enemyManager.ScoutAnalyzing = !enemyManager.ScoutAnalyzing;

            if (analyzeVolume.activeSelf)
            {
                playerShoot.PutWeaponAway();
            }
            else
            {
                playerShoot.PutWeaponBack();
            }
        }
    }
}
