using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ClockScript : MonoBehaviourPun
{
    private int randomHour;
    private int randomMinute;
    private int goalHour;
    private int goalMinute;
    [SerializeField]
    private int clockIndex;
    [SerializeField]
    private TMP_Text time;
    [SerializeField]
    private TMP_Text goalTime;
    [SerializeField]
    private ButtonScript button;
    private bool pressedOnce = false;
    private bool clockComplete = false;
    private ClockManager manager;
    private int startHour;
    private int startMinute;

    private void Start()
    {
        manager = FindObjectOfType<ClockManager>();
    }


    private void StartRandomClock(int randHour, int randMinute, int goHour, int goMinute)
    {
        if (randHour < 0)
        {
            randomHour = Random.Range(1, 24);
            randomMinute = Random.Range(1, 60);
            goalHour = randomHour;
            goalMinute = randomMinute + Random.Range(1, 40);

            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                byte[] times = { (byte)randomHour, (byte)randomMinute, (byte)goalHour, (byte)goalMinute };
                photonView.RPC("StartRandomClockOthers", RpcTarget.Others, times);
            }
        }
        else
        {
            randomHour = randHour;
            randomMinute = randMinute;
            goalHour = goHour;
            goalMinute = goMinute;
        }
       
        startHour = randomHour;
        startMinute = randomMinute;
        pressedOnce = true;

        string goalHourZero = "";
        string goalMinuteZero = "";

        if (goalHour >= 24)
        {
            goalHour -= 24;
        }

        if (goalMinute >= 60)
        {
            goalMinute -= 60;
        }

        if (goalHour >= 0 && goalHour < 10)
        {
            goalHourZero = "0";
        }

        if (goalMinute >= 0 && goalMinute < 10)
        {
            goalMinuteZero = "0";
        }

        goalTime.text = goalHourZero + goalHour + ":" + goalMinuteZero + goalMinute;
    }

    [PunRPC]
    void StartRandomClockOthers(byte[] times)
    {
        StartRandomClock(times[0], times[1], times[2], times[3]);
    }

    public void Clock() 
    {
        if (clockComplete)
        {
            return;
        }

        if (button.IsPressed)
        {
            if (!pressedOnce)
            {
                if (!PhotonNetwork.IsConnected || (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient))
                {
                    StartRandomClock(-1, -1, -1, -1);
                }
            }

            if (!PhotonNetwork.IsConnected || (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient))
            {
                InvokeRepeating("UpdateClockRepeatedly", 0, 1);
            }
            
            UpdateClock(false);
        }
        else if (!button.IsPressed)
        {
            CancelInvoke();

            if (goalHour == randomHour && goalMinute == randomMinute && !clockComplete)
            {
                clockComplete = true;
                manager.CheckAllCompleted(true);
            }

            if (randomMinute > goalMinute || randomHour > goalHour)
            {
                randomHour = startHour;
                randomMinute = startMinute;
            }
        }
    }

    private void UpdateClockRepeatedly()
    {
        UpdateClock(true);
    }

    [PunRPC]
    void UpdateClockOthers(int hour, int min)
    {
        randomHour = hour;
        randomMinute = min;

        UpdateClock(false);
    }

    private void UpdateClock(bool masterClient)
    {
        if (masterClient && PhotonNetwork.IsConnected)
        {
            photonView.RPC("UpdateClockOthers", RpcTarget.Others, randomHour, randomMinute);
        }

        randomMinute++;

        if (randomMinute == 60)
        {
            randomHour++;
            randomMinute = 0;
        }

        if (randomHour >= 24)
        {
            randomHour = 0;
        }

        string extraZeroHour = "";
        string extraZeroMinute = "";

        if (randomHour >= 0 && randomHour < 10)
        {
            extraZeroHour = "0";
        }

        if (randomMinute >= 0 && randomMinute < 10)
        {
            extraZeroMinute = "0";
        }

        time.text = extraZeroHour + randomHour + ":" + extraZeroMinute + randomMinute;
    }
}
