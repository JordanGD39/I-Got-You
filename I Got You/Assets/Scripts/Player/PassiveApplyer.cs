using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PassiveApplyer : MonoBehaviourPun
{
    [SerializeField] private float scoutSpeedBuff = 1.1f;
    [SerializeField] private float supportHealBuff = 1.5f;
    [SerializeField] private float bomberDamageNerf = 0.75f;

    // Start is called before the first frame update
    void Start()
    {
        PlayerStats playerStats = GetComponent<PlayerStats>();

        if (!photonView.IsMine)
        {
            return;
        }

        switch (playerStats.CurrentClass)
        {
            case PlayerStats.ClassNames.SCOUT:
                GetComponent<PlayerMovement>().ModifySpeedStats(scoutSpeedBuff);
                break;
            case PlayerStats.ClassNames.TANK:
                playerStats.HasShieldHealth = true;
                break;
            case PlayerStats.ClassNames.SUPPORT:
                GetComponent<PlayerHealing>().ModifyHealingStat(supportHealBuff);
                break;
            case PlayerStats.ClassNames.BOMBER:
                GetComponent<PlayerShoot>().ModifyDamageStat(bomberDamageNerf);
                break;
        }
    }
}
