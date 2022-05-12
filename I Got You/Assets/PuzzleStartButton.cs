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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inTrigger && Input.GetButtonDown("Interact"))
        {
            manager.ScreenSequence();
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
