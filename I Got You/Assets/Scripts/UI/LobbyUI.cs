using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField roomInput;

    public void CreateRoom()
    {
        if (roomInput.text == "")
        {
            return;
        }

        Debug.Log("LOAD");

        PhotonNetwork.CreateRoom(roomInput.text);
    }

    public void JoinRoom()
    {
        if (roomInput.text == "")
        {
            return;
        }

        PhotonNetwork.JoinRoom(roomInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Protoscene");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }
}
