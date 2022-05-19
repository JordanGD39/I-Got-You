using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleStartButton : MonoBehaviour
{
    [SerializeField]
    private bool isPressed = false;

    [SerializeField]
    private bool inTrigger = false;

    [SerializeField]
    private PuzzleManager manager;

    // Update is called once per frame
    void Update()
    {
        if (manager.RandomInt.Count == 0 && inTrigger && Input.GetButtonDown("Interact") && !manager.OpenDoor)
        {
            manager.ScreenSequence(true);
            // start screen sequence
            isPressed = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        { 
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            inTrigger = false;
        }
    }
}
