using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ClassSelectUI : MonoBehaviour
{
    [SerializeField] private Transform classInfoParent;
    [SerializeField] private GameObject waitText;
    private ClassInfoUI infoUI;
    
    private PlayerStats.ClassNames currentClass = PlayerStats.ClassNames.SCOUT;
    public int NumberOfPlayersChosen = 0;

    private bool hasClicked = false;

    // Start is called before the first frame update
    void Start()
    {
        int thisPlayerIndex = 0;
        string playerName = "YOU";

        if (PhotonFunctionHandler.IsPlayerOnline())
        {
            thisPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

            string nickName = PhotonNetwork.LocalPlayer.NickName;

            if (nickName == "")
            {
                nickName = "Player " + PhotonNetwork.LocalPlayer.ActorNumber;
                PhotonNetwork.LocalPlayer.NickName = nickName;
            }

            playerName = nickName;
        }
        else
        {
            for (int i = 1; i < classInfoParent.childCount; i++)
            {
                classInfoParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        infoUI = classInfoParent.GetChild(thisPlayerIndex).GetComponent<ClassInfoUI>();

        infoUI.FullOpacityInfo();

        infoUI.PlayerName.text = playerName;
        infoUI.UpdateOthersOnNetworkPlayerName();
    }

    public void UpdateCurrentClass(int index)
    {
        hasClicked = true;
        currentClass = (PlayerStats.ClassNames)index;
    }

    public void UpdateClassName()
    {
        if (infoUI == null || !hasClicked)
        {
            return;
        }

        infoUI.UpdateClassName(currentClass);

        if (PhotonFunctionHandler.IsPlayerOnline())
        {
            if (NumberOfPlayersChosen == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    infoUI.SendMasterClientLoadLevelRPC();
                }
                else
                {
                    Invoke("LoadGameScene", 1);
                }                
            }
            else
            {
                waitText.SetActive(true);
            }
        }
        else
        {
            Invoke("LoadGameScene", 1);
        }
    }

    private void LoadGameScene()
    {
        if (PhotonFunctionHandler.IsPlayerOnline())
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        
        PhotonFunctionHandler.LoadSceneAsync("Protoscene");
    }
}
