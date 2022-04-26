using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClockScript : MonoBehaviour
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


    public void Clock() 
    {
        if (button.IsPressed)
        {

            if (!pressedOnce)
            {
                randomHour = Random.Range(1, 24);
                randomMinute = Random.Range(1, 60);
                goalHour = randomHour + Random.Range(1, 4);
                goalMinute = randomMinute + Random.Range(1, 60);
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
            InvokeRepeating("UpdateClock", 0, 1);

            

            UpdateClock();
        }

        else if (!button.IsPressed)
        {
            CancelInvoke();

            if (goalHour == randomHour && goalMinute == randomMinute && !clockComplete)
            {
                manager.FinishedClocks++;
                clockComplete = true;
                manager.CheckAllCompleted();
            }

            if (randomMinute > goalMinute || randomHour > goalHour)
            {
                randomHour = startHour;
                randomMinute = startMinute;
            }
        }
    }

    private void UpdateClock()
    {
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
