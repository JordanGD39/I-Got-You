using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnCanvasToCamera : MonoBehaviour
{
    private Camera cam;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam == null)
        {
            cam = Camera.main;
            canvas.worldCamera = cam;
        }

        transform.LookAt(cam.transform.position);
    }
}
