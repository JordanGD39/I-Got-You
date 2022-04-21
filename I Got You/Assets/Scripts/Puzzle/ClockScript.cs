using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClockScript : MonoBehaviour
{
    private int randomHour;
    private int randomMinute;
    [SerializeField]
    private int clockIndex;
    [SerializeField]
    private TMP_Text time;
    [SerializeField]
    private ButtonScript button;

    public void Clock() 
    {
        if (button.IsPressed)
        {
            randomHour = Random.Range(1, 12);
            randomMinute = Random.Range(1, 60);
            time.text = randomHour + ":" + randomMinute;
        }

        else if (!button.IsPressed)
        {
            Debug.Log("Clock stopped");
        }
    }
}
