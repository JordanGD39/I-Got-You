using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AbilityAdder : MonoBehaviourPun
{
    private PlayerStats playerStats;
    [SerializeField] private GameObject tankShields;
    [SerializeField] private GameObject burstArea;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        AddAbility();
    }

    private void AddAbility()
    {
        switch (playerStats.CurrentClass)
        {
            case PlayerStats.ClassNames.SCOUT:
                gameObject.AddComponent<ScoutAnalyze>();
                break;
            case PlayerStats.ClassNames.TANK:
                TankTaunt tankTaunt = gameObject.AddComponent<TankTaunt>();
                tankTaunt.TankShields = tankShields;
                tankShields.SetActive(false);
                break;
            case PlayerStats.ClassNames.SUPPORT:
                SupportBurstHeal supportBurstHeal = gameObject.AddComponent<SupportBurstHeal>();
                supportBurstHeal.BurstArea = burstArea;
                burstArea.SetActive(false);
                break;
            case PlayerStats.ClassNames.BOMBER:
                break;
        }

        if (playerStats.CurrentClass != PlayerStats.ClassNames.SCOUT)
        {
            GameObject.FindGameObjectWithTag("ScoutVolume").SetActive(false);
        }
    }
}
