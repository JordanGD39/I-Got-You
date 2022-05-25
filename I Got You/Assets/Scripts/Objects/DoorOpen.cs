using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorOpen : MonoBehaviourPun
{
    private PlayerManager playerManager;
    private PlayerUI playerUI;
    [SerializeField] private List<GameObject> playersInRange = new List<GameObject>();
    public List<GameObject> PlayersInRange { get { return playersInRange; } }
    [SerializeField] private GameObject doorToClose;
    [SerializeField] private GameObject model;
    [SerializeField] private bool opened = true;
    public bool Opened { get { return opened; } }
    [SerializeField] private bool openOnly = false;
    [SerializeField] private bool beginOpened = true;
    public bool OpenOnly { get { return openOnly; } set { openOnly = value; } }
    [SerializeField] private bool playerOpen = true;
    public bool PlayerOpen { get { return playerOpen; } set { playerOpen = value; } }
    [SerializeField] private bool allPlayersRequired = true;
    [SerializeField] private Animator openingDoorAnim;
    [SerializeField] private Animator closingDoorAnim;

    public delegate void OpenedDoor();
    public OpenedDoor OnOpenedDoor;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        playerUI = FindObjectOfType<PlayerUI>();

        if (beginOpened)
        {
            OpenDoor();
            opened = true;
        }        
        else
        {
            doorToClose.SetActive(false);
        }

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        playersInRange.Clear();
        Invoke(nameof(OpenResetDelay), 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (opened || !playerOpen || (playerManager != null && PhotonNetwork.IsConnected 
            && playerManager.PlayersInGame.Count < PhotonNetwork.CurrentRoom.PlayerCount))
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other].IsDead)
            {
                return;
            }

            if (allPlayersRequired && playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.ShowNotification("All players are required to open this door");
            }

            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                return;
            }

            playerManager.RemoveMissingPlayers();

            playersInRange.Add(other.gameObject);

            if (playersInRange.Count >= playerManager.Players.Count || !allPlayersRequired)
            {
                OpenDoor();
            }
        }
    }

    public void OpenDoor()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("OpenDoorOthers", RpcTarget.Others);
        }

        playerUI.HideInteractPanel();

        opened = true;
        
        openingDoorAnim.ResetTrigger("Open");
        openingDoorAnim.SetTrigger("Open");

        if (!openOnly)
        {
            doorToClose.SetActive(true);
            closingDoorAnim.ResetTrigger("Close");
            closingDoorAnim.SetTrigger("Close");
        }

        OnOpenedDoor?.Invoke();
    }

    public void OpenClosedDoor()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("OpenClosedDoorOthers", RpcTarget.Others);
        }

        opened = false;

        closingDoorAnim.ResetTrigger("Open");
        closingDoorAnim.SetTrigger("Open");
    }

    [PunRPC]
    void OpenDoorOthers()
    {
        OpenDoor();
    }

    [PunRPC]
    void OpenClosedDoorOthers()
    {
        OpenClosedDoor();
    }

    private void OnTriggerExit(Collider other)
    {
        if (opened || !playerOpen || (playerManager != null && PhotonNetwork.IsConnected
            && playerManager.PlayersInGame.Count < PhotonNetwork.CurrentRoom.PlayerCount))
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.HideInteractPanel();
            }

            if (!playersInRange.Contains(other.gameObject))
            {
                return;
            }
            
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
        playersInRange.Clear();

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
