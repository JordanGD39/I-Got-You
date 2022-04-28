using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AbilityAdder : MonoBehaviourPun
{
    private PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        playerStats = GetComponent<PlayerStats>();

        AddAbility();
    }

    private void AddAbility()
    {
        if (playerStats.CurrentClass != PlayerStats.ClassNames.SCOUT)
        {
            GameObject.FindGameObjectWithTag("ScoutVolume").SetActive(false);
        }

        switch (playerStats.CurrentClass)
        {
            case PlayerStats.ClassNames.SCOUT:
                gameObject.AddComponent<ScoutAnalyze>();
                break;
            case PlayerStats.ClassNames.TANK:
                break;
            case PlayerStats.ClassNames.SUPPORT:
                break;
            case PlayerStats.ClassNames.BOMBER:
                break;
        }
    }
}
