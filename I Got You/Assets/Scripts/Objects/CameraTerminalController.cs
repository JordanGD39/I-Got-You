using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTerminalController : MonoBehaviour
{
    private PlayerManager playerManager;
    [SerializeField] private GameObject terminalCamera;
    private bool controllingCamera = false;
    [SerializeField] private float cameraSpeed = 3;
    [SerializeField] private float cameraBoostSpeed = 12;
    [SerializeField] private Vector2 boundsX;
    [SerializeField] private Vector2 boundsZ;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        terminalCamera.SetActive(false);
        controllingCamera = false;
    }

    private void Update()
    {
        if (controllingCamera)
        {
            UpdateCameraMovement();
        }
    }

    private void UpdateCameraMovement()
    {
        Transform camera = terminalCamera.transform;

        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Normalize();

        movement = camera.TransformDirection(movement);
        movement.y = 0;

        camera.localPosition += cameraSpeed * Time.deltaTime * movement;

        Vector3 restrictedPos = camera.localPosition;

        restrictedPos.x = Mathf.Clamp(restrictedPos.x, boundsX.x, boundsX.y);
        restrictedPos.z = Mathf.Clamp(restrictedPos.z, boundsZ.x, boundsZ.y);

        if (restrictedPos != camera.localPosition)
        {
            camera.localPosition = restrictedPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            PlayerStats playerStats = playerManager.StatsOfAllPlayers[other];

            playerStats.OnInteract = SwitchToCameraControl;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            PlayerStats playerStats = playerManager.StatsOfAllPlayers[other];

            playerStats.OnInteract = null;
        }
    }

    private void SwitchToCameraControl(PlayerStats playerStats)
    {
        terminalCamera.SetActive(true);
        Camera.main.enabled = false;
        playerStats.GetComponent<PlayerMovement>().enabled = false;
        playerStats.GetComponent<PlayerRotation>().enabled = false;
        playerStats.PlayerShootScript.enabled = false;
        controllingCamera = true;
    }
}
