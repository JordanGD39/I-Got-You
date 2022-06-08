using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ColorInput : MonoBehaviourPun
{
    [SerializeField]
    private int codeInput;
    [SerializeField]
    private GameObject monitor;
    private PlayerUI playerUI;
    private MeshRenderer render;
    private PlayerManager playerManager;
    private bool inRange;
    [SerializeField]
    private PuzzleManager manager;

    // Start is called before the first frame update
    void Start()
    {
        playerUI = FindObjectOfType<PlayerUI>();
        render = monitor.GetComponent<MeshRenderer>();
        playerManager = FindObjectOfType<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!manager.OpenDoor && inRange && Input.GetButtonDown("Interact") && manager.ShownPuzzle)
        {
            AddInput(true);
        }
    }

    private void AddInput(bool localPlayer)
    {
        if (localPlayer && PhotonNetwork.IsConnected)
        {
            photonView.RPC("AddInputOthers", RpcTarget.Others);
        }
        render.material.mainTexture = null;

        switch (codeInput)
        {
            case 1:
                render.material.color = Color.red;
                StartCoroutine(WaitTime());
                manager.PlayerInput.Add(codeInput);
                break;
            case 2:
                render.material.color = Color.green;
                StartCoroutine(WaitTime());
                manager.PlayerInput.Add(codeInput);
                break;
            case 3:
                render.material.color = Color.blue;
                StartCoroutine(WaitTime());
                manager.PlayerInput.Add(codeInput);
                break;
            default:
                break;
        }

        manager.CheckCorrectStep();
    }

    [PunRPC]
    void AddInputOthers()
    {
        AddInput(false);
    }

    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(0.5f);
        render.material.color = Color.black;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.ShowInteractPanel(" to input color");
            }

            inRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.HideInteractPanel();
            }

            inRange = false;
        }
    }
}
