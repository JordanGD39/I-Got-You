using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class OnlineGameHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private float timeBeforeDisconnectAfterMcSwitch = 3;
    private PlayerUI playerUI;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Destroy(gameObject);
        }

        playerUI = FindObjectOfType<PlayerUI>();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        playerUI.ShowErrorScreen("Host left the game!\nDisconnecting...");
        Invoke(nameof(LeaveRoom), timeBeforeDisconnectAfterMcSwitch);
    }

    private void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Lobby");
    }
}
