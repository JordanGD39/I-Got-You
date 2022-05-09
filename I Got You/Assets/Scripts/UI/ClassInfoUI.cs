using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class ClassInfoUI : MonoBehaviourPun
{
    [SerializeField] private Image classLogo;
    [SerializeField] private TextMeshProUGUI playerName;
    public TextMeshProUGUI PlayerName { get { return playerName; } }
    [SerializeField] private TextMeshProUGUI className;
    private bool fullOpacitySet = false;
    
    private ClassSelectUI selectUI;
    private bool hasChosen = false;

    // Start is called before the first frame update
    void Start()
    {
        selectUI = FindObjectOfType<ClassSelectUI>();

        if (fullOpacitySet)
        {
            return;
        }

        Color color = classLogo.color;
        color.a = 0.5f;
        classLogo.color = color;
        playerName.color = color;
        className.color = color;
        className.gameObject.SetActive(false);
    }

    public void FullOpacityInfo()
    {
        fullOpacitySet = true;

        Color color = classLogo.color;
        color.a = 1;
        classLogo.color = color;
        playerName.color = color;
        className.color = color;
        className.gameObject.SetActive(true);
    }

    public void UpdateClassName(PlayerStats.ClassNames theClassName)
    {
        string editedClassName = UpperCaseFirstLetter(System.Enum.GetName(typeof(PlayerStats.ClassNames), (int)theClassName));

        int playerIndex = PhotonNetwork.IsConnected ? (PhotonNetwork.LocalPlayer.ActorNumber - 1) : 0;

        if (!PlayerChoiceManager.instance.ChosenClasses.ContainsKey(playerIndex))
        {
            PlayerChoiceManager.instance.ChosenClasses.Add(playerIndex, theClassName);
        }
        else
        {
            PlayerChoiceManager.instance.ChosenClasses[playerIndex] = theClassName;
        }

        className.text = editedClassName;

        if (!hasChosen)
        {
            selectUI.NumberOfPlayersChosen++;
        }

        hasChosen = true;

        if (PhotonNetwork.IsConnected)
        {
            int index = (int)theClassName;
            photonView.RPC("UpdateClassNameOther", RpcTarget.OthersBuffered, (byte)index, (byte)PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    private string UpperCaseFirstLetter(string input)
    {
        input = input.ToLower();

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    [PunRPC]
    void UpdateClassNameOther(byte classIndex, byte actorIndex)
    {
        Debug.Log(classIndex);

        if (!hasChosen)
        {
            selectUI.NumberOfPlayersChosen++;
        }

        hasChosen = true;

        PlayerStats.ClassNames theName = (PlayerStats.ClassNames)classIndex;

        actorIndex -= 1;

        if (!PlayerChoiceManager.instance.ChosenClasses.ContainsKey(actorIndex))
        {
            PlayerChoiceManager.instance.ChosenClasses.Add(actorIndex, theName);
        }
        else
        {
            PlayerChoiceManager.instance.ChosenClasses[actorIndex] = theName;
        }

        string editedClassName = UpperCaseFirstLetter(System.Enum.GetName(typeof(PlayerStats.ClassNames), classIndex));

        className.text = editedClassName;
    }

    public void UpdateOthersOnNetworkPlayerName()
    {
        if (PhotonFunctionHandler.IsPlayerOnline())
        {
            photonView.RPC("UpdatePlayerNameOther", RpcTarget.OthersBuffered, (byte)PhotonNetwork.LocalPlayer.ActorNumber);
        }        
    }

    [PunRPC]
    void UpdatePlayerNameOther(byte actorIndex)
    {
        playerName.text = PhotonNetwork.CurrentRoom.Players[actorIndex].NickName;
        FullOpacityInfo();
    }

    public void SendMasterClientLoadLevelRPC()
    {
        photonView.RPC("MasterClientLoadLevel", RpcTarget.Others);
    }

    [PunRPC]
    void MasterClientLoadLevel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonFunctionHandler.LoadSceneAsync("Protoscene");
        }
    }
}