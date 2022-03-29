using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ConnectToNetwork : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();        
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.SendRate = 10;
        PhotonNetwork.SerializationRate = 10;
        Debug.Log(PhotonNetwork.CountOfPlayers);
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
