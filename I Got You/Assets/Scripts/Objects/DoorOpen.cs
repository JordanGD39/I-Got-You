using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorOpen : MonoBehaviourPun
{
    private PlayerManager playerManager;
    [SerializeField] private List<GameObject> playersInRange = new List<GameObject>();
    [SerializeField] private GameObject doorToClose;
    [SerializeField] private GameObject model;
    [SerializeField] private bool opened = true;
    [SerializeField] private Animator openingDoorAnim;
    [SerializeField] private Animator closingDoorAnim;

    public delegate void OpenedDoor();
    public OpenedDoor OnOpenedDoor;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        doorToClose.SetActive(false);
        opened = true;

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        playersInRange.Clear();
        Invoke(nameof(OpenResetDelay), 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (opened || (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient))
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other].IsDead)
            {
                return;
            }

            playerManager.RemoveMissingPlayers();

            playersInRange.Add(other.gameObject);

            if (playersInRange.Count >= playerManager.Players.Count)
            {
                OpenDoor();
            }
        }
    }

    private void OpenDoor()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("OpenDoorOthers", RpcTarget.Others);
        }

        opened = true;
        doorToClose.SetActive(true);
        openingDoorAnim.ResetTrigger("Open");
        openingDoorAnim.SetTrigger("Open");
        playersInRange.Clear();
        closingDoorAnim.ResetTrigger("Close");
        closingDoorAnim.SetTrigger("Close");
        OnOpenedDoor?.Invoke();
    }

    [PunRPC]
    void OpenDoorOthers()
    {
        OpenDoor();
    }

    private void OnTriggerExit(Collider other)
    {
        if (opened || (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient))
        {
            return;
        }

        if (other.CompareTag("PlayerCol") && playersInRange.Contains(other.gameObject))
        {
            playersInRange.Remove(other.gameObject);
        }
    }

    public void CloseOpeningDoor()
    {
        model.SetActive(false);
        model.SetActive(true);

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("CloseOpeningDoorOthers", RpcTarget.Others);
        }
    }

    [PunRPC]
    void CloseOpeningDoorOthers()
    {
        CloseOpeningDoor();
    }

    public void ResetDoor()
    {
        doorToClose.SetActive(false);
        model.SetActive(false);
        model.SetActive(true);

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetDoorOthers", RpcTarget.Others);
        }

        Invoke(nameof(OpenResetDelay), 0.5f);
    }

    [PunRPC]
    void ResetDoorOthers()
    {
        ResetDoor();
    }

    private void OpenResetDelay()
    {
        opened = false;
    }
}
