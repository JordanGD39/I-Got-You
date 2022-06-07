using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TankTaunt : MonoBehaviourPun
{
    private EnemyManager enemyManager;
    [SerializeField] private float damageMultiplier = 0.4f;
    [SerializeField] private float tauntTime = 3;
    public float DamageMultiplier { get { return damageMultiplier; } }
    public bool Taunting { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<PlayerStats>().TankTauntScript = this;
        enemyManager = FindObjectOfType<EnemyManager>();
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
        if (Input.GetButtonDown("Ability"))
        {
            SetTaunt(true);
            Invoke(nameof(StopTaunt), tauntTime);
        }
    }

    private void SetTaunt(bool active)
    {
        Taunting = active;

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

    private void StopTaunt()
    {
        SetTaunt(false);
    }
}
