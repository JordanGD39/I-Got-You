using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private int floor = 0;
    [SerializeField] private float fadeTime = 1;
    public int Floor { get { return floor; } }
    public int TotalArenaRooms { get; set; } = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void AddFloorLevel()
    {
        floor++;

        DifficultyManager difficultyManager = DifficultyManager.instance;

        if (difficultyManager.DifficultyLevel < Floor * TotalArenaRooms)
        {
            difficultyManager.DifficultyLevel = Floor * TotalArenaRooms;
        }
    }

    public void ResetFloorLevel()
    {
        floor = 0;
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayersStatsHolder.instance.ClearSavedStats();
        ResetFloorLevel();
        DifficultyManager.instance.DifficultyLevel = 0;
        FindObjectOfType<PlayerUI>().FadeIn.SetActive(true);
        Invoke(nameof(LoadLobby), fadeTime);
    }

    private void LoadLobby()
    {
        SceneManager.LoadSceneAsync("Lobby");
    }
}
