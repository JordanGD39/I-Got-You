using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class SettingsUI : MonoBehaviourPun
{
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValueText;

    public delegate void ChangedSensitivity(int val);
    public ChangedSensitivity OnChangedSensitivity;

    // Start is called before the first frame update
    void Start()
    {
        sensitivitySlider.value = PlayerPrefs.GetInt("Sensitivity", 6);
    }

    public void ChangeSensitivity()
    {
        int val = Mathf.RoundToInt(sensitivitySlider.value);
        sensitivityValueText.text = val.ToString();
        PlayerPrefs.SetInt("Sensitivity", val);
        OnChangedSensitivity?.Invoke(val);
    }

    public void Disconnect()
    {
        LeaveRoom();
    }

    private void LeaveRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Lobby");
    }
}
