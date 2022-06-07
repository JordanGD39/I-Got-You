using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerRevive : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float deathTimer = 0;
    [SerializeField] private float timeToDie = 15;
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private float resetDamageTime = 1;
    [SerializeField] private float damageTimer = 0;
    [SerializeField] private float syncTimer = 0;
    [SerializeField] private float lerpSpeedSync = 2;
    [SerializeField] private Camera deathCam;
    [SerializeField] private GameObject revivePanel;
    [SerializeField] private Image reviveCircle;

    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private PlayerRotation playerRotation;
    private PlayerShoot playerShoot;
    private PlayerHealing playerHealing;
    private PlayerManager playerManager;
    private Camera cam;

    private bool timerStarted = false;
    private float currentMultiplier = 1;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(deathTimer);
        }
        else if (stream.IsReading)
        {
            syncTimer = (float)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        revivePanel.SetActive(false);

        playerManager = FindObjectOfType<PlayerManager>();
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
        playerRotation = GetComponent<PlayerRotation>();
        playerShoot = GetComponent<PlayerShoot>();
        playerHealing = GetComponent<PlayerHealing>();
        deathCam.gameObject.SetActive(false);

        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            cam = Camera.main;
            revivePanel.transform.parent.gameObject.SetActive(false);
        }
    }

    public void StartTimer()
    {
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("ShowReviveOthers", RpcTarget.Others);
        }

        deathTimer = timeToDie;
        damageTimer = 0;
        timerStarted = true;
        playerRotation.StartLerpToResetPos();
        playerHealing.StopAllCoroutines();
        playerHealing.StopHealing();
        playerHealing.enabled = false;
    }

    [PunRPC]
    void ShowReviveOthers()
    {
        revivePanel.SetActive(true);
        reviveCircle.fillAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            reviveCircle.fillAmount = Mathf.Lerp(reviveCircle.fillAmount, syncTimer / timeToDie, lerpSpeedSync * Time.deltaTime);
            return;
        }

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
        playerStats.IsDead = true;

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
