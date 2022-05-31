using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorOpen : MonoBehaviourPun
{
    private PlayerManager playerManager;
    private PlayerUI playerUI;
    private AudioSource doorSfx;
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
    [SerializeField] private bool canOpen = true;
    [SerializeField] private Animator openingDoorAnim;
    [SerializeField] private Animator closingDoorAnim;
    [SerializeField] private GenerationRoomData generationRoomData;
    [SerializeField] private int openingIndex = -1;

    public delegate void OpenedDoor();
    public OpenedDoor OnOpenedDoor;

    // Start is called before the first frame update
    void Start()
    {
        doorSfx = GetComponent<AudioSource>();
        playerManager = FindObjectOfType<PlayerManager>();
        playerUI = FindObjectOfType<PlayerUI>();

        DungeonGenerator dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGenerator == null)
        {
            dungeonGenerator.OnGenerationDone += CheckIfDoorLeadsToHallway;
        }
        else
        {
            CheckIfDoorLeadsToHallway();
        }

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

    private void CheckIfDoorLeadsToHallway()
    {
        if (generationRoomData == null || openingIndex < 0)
        {
            return;
        }

        canOpen = generationRoomData.ChosenOpenings.Contains(generationRoomData.Openings[openingIndex]);
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
        if (!canOpen)
        {
            return;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("OpenDoorOthers", RpcTarget.Others, canOpen, openOnly);
        }

        doorSfx.Play();

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
        if (!canOpen)
        {
            return;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("OpenClosedDoorOthers", RpcTarget.Others, canOpen);
        }

        opened = false;

        closingDoorAnim.ResetTrigger("Open");
        closingDoorAnim.SetTrigger("Open");
    }

    [PunRPC]
    void OpenDoorOthers(bool openingPossible, bool onlyOpen)
    {
        canOpen = openingPossible;
        openOnly = onlyOpen;
        OpenDoor();
    }

    [PunRPC]
    void OpenClosedDoorOthers(bool openingPossible)
    {
        canOpen = openingPossible;
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
        if (!canOpen)
        {
            return;
        }

        model.SetActive(false);
        model.SetActive(true);

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("CloseOpeningDoorOthers", RpcTarget.Others, canOpen);
        }
    }

    [PunRPC]
    void CloseOpeningDoorOthers(bool openingPossible)
    {
        canOpen = openingPossible;
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
