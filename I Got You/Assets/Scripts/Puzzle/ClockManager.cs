using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ClockManager : MonoBehaviourPun
{
    [SerializeField]
    private List<GameObject> clockObject;
    [SerializeField] private RoomDoorOpener doorOpener;
    private int finishedClocks = 0;

    public void CheckAllCompleted(bool localPlayer)
    {
        if (PhotonNetwork.IsConnected && localPlayer)
        {
            photonView.RPC("CheckAllCompletedOthers", RpcTarget.Others);
        }

        finishedClocks++;

        if (finishedClocks >= clockObject.Count)
        {
            doorOpener.CheckCompletion();
        }
    }

    [PunRPC]
    void CheckAllCompletedOthers()
    {
        CheckAllCompleted(false);
    }
}
