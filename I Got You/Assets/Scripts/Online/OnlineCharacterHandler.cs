using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

public class OnlineCharacterHandler : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        bool isPlayer = gameObject.CompareTag("Player");
        bool checkMasterOrMine = photonView.IsMine;

        if (!isPlayer)
        {
            checkMasterOrMine = PhotonNetwork.IsMasterClient;
        }

        if (checkMasterOrMine || !PhotonNetwork.IsConnected)
        {
            Destroy(this);
            return;
        }

        MonoBehaviour[] scripts =  GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour item in scripts)
        {
            if (item is PhotonView) { continue; }
            if (item is PhotonVoiceView) { continue; }
            if (item is Recorder) { continue; }
            if (item is PlayerStats){ continue; }
            if (item is SyncMovement){ continue; }
            if (item is EnemyStats){ continue; }
            if (item is PlayerRevive){ continue; }
            if (item is PlayerShoot){ continue; }
            if (item is PlayerHealing){ continue; }

            if (item != this)
            {
                Destroy(item);
            }                
        }

        if (!isPlayer)
        {
            return;
        }

        GetComponentInChildren<AudioListener>().enabled = false;

        Camera[] cams = GetComponentsInChildren<Camera>();

        foreach (Camera cam in cams)
        {
            cam.enabled = false;

            if (cam.CompareTag("MainCamera"))
            {
                cam.gameObject.layer = 3;
                cam.transform.GetChild(0).gameObject.layer = 3;
            }
        }
    }
}
