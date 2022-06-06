using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorHoldPlayer : MonoBehaviour
{
    private bool holdLocalPlayer = true;
    private bool ableToExit = false;
    private PlayerManager playerManager;
    private CharacterController localCharacterController;

    [SerializeField] private float maxDistance = 2;
    [SerializeField] private float yPos = 1.33f;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        localCharacterController = playerManager.LocalPlayer.GetComponent<CharacterController>();

        DungeonGenerator dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGenerator.StartingRoom == null)
        {
            dungeonGenerator.OnGenerationDone += InvokeAbleToExitElevator;
        }
        else
        {
            InvokeAbleToExitElevator();
        }
    }

    private void InvokeAbleToExitElevator()
    {
        Invoke(nameof(AbleToExitElevator), 1);
    }

    private void AbleToExitElevator()
    {
        ableToExit = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (holdLocalPlayer)
        {
            Transform localPlayer = localCharacterController.transform;

            Vector3 playerPos = localPlayer.position;
            playerPos.y = transform.position.y;

            float dist = Vector3.Distance(playerPos, transform.position);
            Debug.Log(dist);

            if (playerPos.y < yPos)
            {
                playerPos.y = yPos;
                localPlayer.position = playerPos;
            }

            if (dist > maxDistance)
            {
                //localCharacterController.enabled = false;
                Vector3 dir = playerPos - transform.position;

                Vector3 teleportPos = transform.position + maxDistance * dir.normalized;
                teleportPos.y = localPlayer.transform.position.y;
                localPlayer.position = teleportPos;

                //localCharacterController.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ableToExit && other.CompareTag("PlayerCol") && playerManager.LocalPlayer == playerManager.StatsOfAllPlayers[other])
        {
            holdLocalPlayer = false;
        }
    }
}
