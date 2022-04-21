using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField]
    private bool isPressed = false;
    public bool IsPressed { get { return isPressed; } }
    [SerializeField]
    private int buttonIndex;
    [SerializeField]
    private GameObject clock;


    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isPressed =! isPressed;
            Debug.Log("Clock starting!");
            clock.GetComponent<ClockScript>().Clock();
        }
    }
}
