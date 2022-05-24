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
    private MeshRenderer render;
    private bool inRange;
    [SerializeField]
    private PuzzleManager manager;

    // Start is called before the first frame update
    void Start()
    {
        render = monitor.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!manager.OpenDoor && inRange && Input.GetButtonDown("Interact"))
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
        yield return new WaitForSeconds(2f);
        render.material.color = Color.black;
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
