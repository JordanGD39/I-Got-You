using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField roomInput;
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private GameObject classSelectPanel;
    [SerializeField] private TextMeshProUGUI connectingToRoomText;

    private void Start()
    {
        if (!PhotonFunctionHandler.IsPlayerOnline())
        {
            classSelectPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void CreateRoom()
    {
        if (roomInput.text == "")
        {
            return;
        }

        Debug.Log("LOAD");

        PhotonNetwork.LocalPlayer.NickName = playerNameInput.text;
        PhotonNetwork.CreateRoom(roomInput.text);

        connectingToRoomText.text = "Creating room...";
        connectingToRoomText.gameObject.SetActive(true);
    }


    public void JoinRoom()
    {
        if (roomInput.text == "")
        {
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = playerNameInput.text;
        PhotonNetwork.JoinRoom(roomInput.text);

        connectingToRoomText.text = "Joining room...";
        connectingToRoomText.gameObject.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        classSelectPanel.SetActive(true);
        gameObject.SetActive(false);

        PhotonNetwork.CurrentRoom.MaxPlayers = 4;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        connectingToRoomText.text = message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        connectingToRoomText.text = message;
    }
}
