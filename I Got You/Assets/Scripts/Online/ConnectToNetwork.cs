using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ConnectToNetwork : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start_void()
    {
        PhotonNetwork.ConnectUsingSettings();        
    }

    public override void OnConnectedToMaster_void()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log(PhotonNetwork.CountOfPlayers);
    }

    public override void OnJoinedLobby_void()
    {
        SceneManager.LoadScene("Lobby");
    }
}
