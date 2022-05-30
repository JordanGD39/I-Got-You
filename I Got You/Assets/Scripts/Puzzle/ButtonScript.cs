using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ButtonScript : MonoBehaviourPun
{
    [SerializeField]
    private bool isPressed = false;
    public bool IsPressed { get { return isPressed; } }
    private bool hasTime = false;
    public bool HasTime { get { return hasTime; } }
    [SerializeField]
    private int buttonIndex;
    [SerializeField]
    private ClockScript clock;
    private Animator anim;
    private PlayerUI playerUI;
    private PlayerManager playerManager;

    private void Start()
    {
        playerUI = FindObjectOfType<PlayerUI>();
        playerManager = FindObjectOfType<PlayerManager>();
        anim = GetComponent<Animator>();
    }

    private bool inRange = false;

    private void Update()
    {
        if (inRange && Input.GetButtonDown("Interact"))
        {
            StartClock(true);
          //  clockObject[buttonIndex].GetComponent<ClockScript>().Clock();
        }
    }

    private void StartClock(bool localPlayer)
    {
        if (localPlayer && PhotonNetwork.IsConnected)
        {
            photonView.RPC("StartClockOthers", RpcTarget.Others);
        }

        playerUI.HideInteractPanel();
        isPressed = !isPressed;
        hasTime = true;
        anim.Play("Pressed");
        Debug.Log("Clock starting!");
        clock.Clock();
    }

    [PunRPC]
    void StartClockOthers()
    {
        StartClock(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.ShowInteractPanel(!isPressed ? " to start the clock" : " to pause the clock");
            }
                
            inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.HideInteractPanel();
            }
            inRange = false;
        }
    }
}
