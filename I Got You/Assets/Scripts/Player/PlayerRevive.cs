using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerRevive : MonoBehaviourPun
{
    [SerializeField] private float deathTimer = 0;
    [SerializeField] private float timeToDie = 15;
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private float resetDamageTime = 1;
    [SerializeField] private float damageTimer = 0;
    [SerializeField] private Camera deathCam;

    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private PlayerRotation playerRotation;
    private PlayerShoot playerShoot;
    private PlayerManager playerManager;
    private Camera cam;

    private bool timerStarted = false;
    private float currentMultiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
        playerRotation = GetComponent<PlayerRotation>();
        playerShoot = GetComponent<PlayerShoot>();
        deathCam.gameObject.SetActive(false);

        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            cam = Camera.main;
        }
    }

    public void StartTimer()
    {
        deathTimer = timeToDie;
        damageTimer = 0;
        timerStarted = true;
        playerRotation.StartLerpToResetPos();
    }

    // Update is called once per frame
    void Update()
    {
        if (!timerStarted)
        {
            return;
        }

        if (deathTimer > 0)
        {
            deathTimer -= currentMultiplier * Time.deltaTime;
        }
        else
        {
            BledOut();

            if (PhotonNetwork.IsConnected && photonView.IsMine)
            {
                photonView.RPC(nameof(PlayerDiedForOthersRPC), RpcTarget.Others);
            }
        }

        if (damageTimer < resetDamageTime)
        {
            damageTimer += Time.deltaTime;
        }
        else
        {
            currentMultiplier = 1;
        }
    }

    public void ResetDamageTimer()
    {
        damageTimer = 0;
        currentMultiplier = damageMultiplier;
    }

    private void BledOut()
    {
        timerStarted = false;
        playerManager.DeadPlayers.Add(playerStats);
        playerManager.Players.Remove(playerStats);

        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            playerRotation.OnGiveInputBack = DisableComponents;
            playerRotation.StartLerpToResetPos();
        }        
    }

    private void DisableComponents()
    {
        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            cam.enabled = false;
            deathCam.gameObject.SetActive(true);
            playerMovement.enabled = false;
            playerRotation.enabled = false;
            playerShoot.enabled = false;
        }
    }

    [PunRPC]
    void PlayerDiedForOthersRPC()
    {
        BledOut();
    }
}
