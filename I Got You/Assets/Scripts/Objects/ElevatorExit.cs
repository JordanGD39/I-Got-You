using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ElevatorExit : MonoBehaviourPun
{
    private PlayerManager playerManager;
    private DungeonGenerator dungeonGenerator;
    private PlayerUI playerUI;
    private FadeScreen fadeScreen;
    private List<GameObject> playersInRange = new List<GameObject>();
    [SerializeField] private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        fadeScreen = FindObjectOfType<FadeScreen>();
        playerUI = FindObjectOfType<PlayerUI>();
        playerManager = FindObjectOfType<PlayerManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ShutElevatorDoors();
        }
    }

    private void ShutElevatorDoors()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ShutElevatorDoorsOthers", RpcTarget.Others);
        }

        anim.SetTrigger("Close");
        fadeScreen.TriggerFadeIn();

        foreach (PlayerStats player in playerManager.PlayersInGame)
        {
            player.SavePlayerStats();
        }

        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            Invoke("ReloadScene", 1);
        }        
    }

    private void ReloadScene()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ReloadSceneOthers", RpcTarget.Others);
        }

        PhotonFunctionHandler.LoadSceneAsync("GameScene");
    }

    [PunRPC]
    void ReloadSceneOthers()
    {
        ReloadScene();
    }

    [PunRPC]
    void ShutElevatorDoorsOthers()
    {
        ShutElevatorDoors();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerManager != null && PhotonNetwork.IsConnected
            && playerManager.PlayersInGame.Count < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other].IsDead)
            {
                return;
            }

            if (playerManager.StatsOfAllPlayers[other] == playerManager.LocalPlayer)
            {
                playerUI.ShowNotification("All players are required to start the elevator");
            }

            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                return;
            }

            playerManager.RemoveMissingPlayers();

            playersInRange.Add(other.gameObject);

            if (playersInRange.Count >= playerManager.Players.Count)
            {
                ShutElevatorDoors();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerManager != null && PhotonNetwork.IsConnected
            && playerManager.PlayersInGame.Count < PhotonNetwork.CurrentRoom.PlayerCount)
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
}
