using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTerminalController : InteractableObject
{
    [SerializeField] private GameObject terminalCamera;
    private bool controllingCamera = false;
    [SerializeField] private float cameraSpeed = 3;
    [SerializeField] private float cameraBoostSpeed = 12;
    [SerializeField] private Vector2 boundsX;
    [SerializeField] private Vector2 boundsZ;
    [SerializeField] private float timeToCloseCam = 0.5f;

    private PlayerStats playerStatsControllingCam;
    private float timer = 0;
    private Camera playerCam;

    // Start is called before the first frame update
    protected override void AfterStart()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        terminalCamera.SetActive(false);
        controllingCamera = false;
    }

    private void Update()
    {
        if (controllingCamera)
        {
            if (timer < timeToCloseCam)
            {
                timer += Time.deltaTime;
            }
            else
            {
                CheckStopLookingTroughCamera();
            }
            
            UpdateCameraMovement();
        }
    }

    private void CheckStopLookingTroughCamera()
    {
        if (Input.GetButtonDown("Interact"))
        {
            SetCameraControl(playerStatsControllingCam, false);
        }
    }

    private void UpdateCameraMovement()
    {
        Transform camera = terminalCamera.transform;

        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Normalize();

        movement = camera.TransformDirection(movement);
        movement.y = 0;

        float speed = 0;

        speed = Input.GetButton("Sprint") ? cameraBoostSpeed : cameraSpeed;

        camera.localPosition += speed * Time.deltaTime * movement;

        Vector3 restrictedPos = camera.localPosition;

        restrictedPos.x = Mathf.Clamp(restrictedPos.x, boundsX.x, boundsX.y);
        restrictedPos.z = Mathf.Clamp(restrictedPos.z, boundsZ.x, boundsZ.y);

        if (restrictedPos != camera.localPosition)
        {
            camera.localPosition = restrictedPos;
        }
    }

    protected override void PlayerTriggerEntered(PlayerStats playerStats)
    {
        base.PlayerTriggerEntered(playerStats);

        playerStats.OnInteract += SwitchToCameraControl;
    }

    private void SwitchToCameraControl(PlayerStats playerStats)
    {
        playerStatsControllingCam = playerStats;
        playerCam = Camera.main;
        playerStats.OnInteract = null;
        SetCameraControl(playerStats, true);
    }

    private void SetCameraControl(PlayerStats playerStats, bool activeCamera)
    {
        terminalCamera.SetActive(activeCamera);
        playerCam.enabled = !activeCamera;
        playerStats.GetComponent<PlayerMovement>().enabled = !activeCamera;
        playerStats.GetComponent<PlayerRotation>().enabled = !activeCamera;
        playerStats.PlayerShootScript.enabled = !activeCamera;
        controllingCamera = activeCamera;
    }
}
