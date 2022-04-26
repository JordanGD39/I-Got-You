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
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private bool inRange = false;

    private void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            isPressed = !isPressed;
            anim.Play("Pressed");
            Debug.Log("Clock starting!");
            clock.GetComponent<ClockScript>().Clock();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            inRange = false;
        }
    }
}
