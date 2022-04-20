using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorOpen : MonoBehaviourPun
{
    private PlayerManager playerManager_PlayerManager;
    [SerializeField] private List<GameObject> playersInRange_List_GameObject = new List<GameObject>();
    [SerializeField] private GameObject doorToClose_GameObject;
    [SerializeField] private GameObject model_GameObject;
    [SerializeField] private bool opened_bool = true;
    [SerializeField] private Animator openingDoorAnim_Animator;
    [SerializeField] private Animator closingDoorAnim_Animator;

    public delegate void OpenedDoor();
    public OpenedDoor OnOpenedDoor_OpenedDoor;

    // Start is called before the first frame update
    void Start_void()
    {
        playerManager_PlayerManager = FindObjectOfType<PlayerManager>();
        doorToClose_GameObject.SetActive(false);
        opened_bool = true;

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        playersInRange_List_GameObject.Clear();
        Invoke(nameof(OpenResetDelay_void), 0.5f);
    }

    private void OnTriggerEnter_void(Collider other)
    {
        if (opened_bool || (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient))
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager_PlayerManager.StatsOfAllPlayers_Dictionary[other].IsDead)
            {
                return;
            }

            playersInRange_List_GameObject.Add(other.gameObject);

            if (playersInRange_List_GameObject.Count >= playerManager_PlayerManager.Players.Count)
            {
                OpenDoor_void();
            }
        }
    }

    private void OpenDoor_void()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("OpenDoorOthers", RpcTarget.Others);
        }

        opened_bool = true;
        doorToClose_GameObject.SetActive(true);
        openingDoorAnim_Animator.ResetTrigger("Open");
        openingDoorAnim_Animator.SetTrigger("Open");
        playersInRange_List_GameObject.Clear();
        closingDoorAnim_Animator.ResetTrigger("Close");
        closingDoorAnim_Animator.SetTrigger("Close");
        OnOpenedDoor_OpenedDoor?.Invoke();
    }

    [PunRPC]
    void OpenDoorOthers_void()
    {
        OpenDoor_void();
    }

    private void OnTriggerExit_void(Collider other)
    {
        if (opened_bool || (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient))
        {
            return;
        }

        if (other.CompareTag("PlayerCol") && playersInRange_List_GameObject.Contains(other.gameObject))
        {
            playersInRange_List_GameObject.Remove(other.gameObject);
        }
    }

    public void CloseOpeningDoor_void()
    {
        model_GameObject.SetActive(false);
        model_GameObject.SetActive(true);

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("CloseOpeningDoorOthers", RpcTarget.Others);
        }
    }

    [PunRPC]
    void CloseOpeningDoorOthers_void()
    {
        CloseOpeningDoor_void();
    }

    public void ResetDoor_void()
    {
        doorToClose_GameObject.SetActive(false);
        model_GameObject.SetActive(false);
        model_GameObject.SetActive(true);

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetDoorOthers", RpcTarget.Others);
        }

        Invoke(nameof(OpenResetDelay_void), 0.5f);
    }

    [PunRPC]
    void ResetDoorOthers_void()
    {
        ResetDoor_void();
    }

    private void OpenResetDelay_void()
    {
        opened_bool = false;
    }
}
