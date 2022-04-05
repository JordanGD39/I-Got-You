using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public static class PhotonFunctionHandler
{
    public static GameObject InstantiateGameObject(GameObject theObject, Vector3 pos, Quaternion rotation)
    {
        if (IsPlayerOnline())
        {
            return PhotonNetwork.Instantiate(theObject.name, pos, rotation);
        }
        else
        {
            return Object.Instantiate(theObject, pos, rotation);
        }
    }

    public static bool IsPlayerOnline()
    {
        return !PhotonNetwork.OfflineMode && (PhotonNetwork.InRoom || PhotonNetwork.InLobby);
    }

    public static void LoadSceneAsync(string sceneName)
    {
        if (IsPlayerOnline())
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
