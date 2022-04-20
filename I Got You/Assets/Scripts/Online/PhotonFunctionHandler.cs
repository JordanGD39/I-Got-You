using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public static class PhotonFunctionHandler
{
    public static GameObject InstantiateGameObject_GameObject(GameObject theObject, Vector3 pos, Quaternion rotation)
    {
        if (IsPlayerOnline_bool())
        {
            return PhotonNetwork.Instantiate(theObject.name, pos, rotation);
        }
        else
        {
            return Object.Instantiate(theObject, pos, rotation);
        }
    }

    public static bool IsPlayerOnline_bool()
    {
        return !PhotonNetwork.OfflineMode && (PhotonNetwork.InRoom || PhotonNetwork.InLobby);
    }

    public static void LoadSceneAsync_void(string sceneName)
    {
        if (IsPlayerOnline_bool())
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
