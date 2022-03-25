using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 2;
    private float xRotation = 0;

    [SerializeField] private Transform cam;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        Vector2 input = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        input *= rotationSpeed * Time.deltaTime;

        xRotation -= input.y;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        cam.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * input.x);
    }
}
