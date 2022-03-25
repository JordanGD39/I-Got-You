using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField roomInput_InputField;

    public void CreateRoom()
    {
        if (roomInput_InputField.text == "")
        {
            return;
        }

        PhotonNetwork.CreateRoom(roomInput_InputField.text);
    }

    public void JoinRoom()
    {
        if (roomInput_InputField.text == "")
        {
            return;
        }

        PhotonNetwork.JoinRoom(roomInput_InputField.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Protoscene");
    }
}
