using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleStartButton : MonoBehaviour
{
    [SerializeField]
    private bool isPressed = false;
    [SerializeField]
    private PuzzleManager manager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isPressed && Input.GetKeyDown(KeyCode.E))
        {
            manager.ScreenSequence();
            // start screen sequence
            isPressed = true;
        }
    }
}
