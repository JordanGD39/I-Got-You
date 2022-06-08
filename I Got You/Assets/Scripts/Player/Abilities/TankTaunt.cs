using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TankTaunt : MonoBehaviourPun
{
    private EnemyManager enemyManager;
    private PlayerUI playerUI;
    private float damageMultiplier = 0.4f;
    private float tauntTime = 20;
    public GameObject TankShields { get; set; } = null;
    public float DamageMultiplier { get { return damageMultiplier; } }
    public bool Taunting { get; private set; } = false;

    private float charge = 0;
    public float Charge { get { return charge; } }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<PlayerStats>().TankTauntScript = this;
        enemyManager = FindObjectOfType<EnemyManager>();
        playerUI = FindObjectOfType<PlayerUI>();

        if (PlayersStatsHolder.instance.PlayerStatsSaved.Length > 0 && PlayersStatsHolder.instance.PlayerStatsSaved[!PhotonNetwork.IsConnected ? 0 : photonView.OwnerActorNr - 1] != null)
        {
            charge = PlayersStatsHolder.instance.PlayerStatsSaved[!PhotonNetwork.IsConnected ? 0 : photonView.OwnerActorNr - 1].abilityCharge;
        }

        playerUI.UpdateAbility(1, charge, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine || Taunting)
        {
            return;
        }

        CheckAbilityToggle();
    }

    private void CheckAbilityToggle()
    {
        if (Input.GetButtonDown("Ability") && charge >= 1)
        {
            SetTaunt(true);
            playerUI.UpdateAbility(1, 0, 1);
            charge = 0;
            Invoke(nameof(StopTaunt), tauntTime);
        }
    }

    public void AddCharge(float add)
    {
        charge += add;
        playerUI.UpdateAbility(1, charge, 1);
    }

    private void SetTaunt(bool active)
    {
        Taunting = active;
        TankShields.SetActive(active);

        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("SetActiveShieldsOthers", RpcTarget.Others, active);
        }

        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            enemyManager.EnemiesTarget = active ? transform : null;
        }
        else
        {
            photonView.RPC("SetTargetMasterClient", RpcTarget.MasterClient, active);
        }
    }

    [PunRPC]
    void SetTargetMasterClient(bool taunt)
    {
        enemyManager.EnemiesTarget = taunt ? transform : null;
    }

    [PunRPC]
    void SetActiveShieldsOthers(bool taunt)
    {
        TankShields.SetActive(taunt);
    }

    private void StopTaunt()
    {
        SetTaunt(false);
    }
}
