using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerOnlineHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonView view = GetComponent<PhotonView>();

        if (!view.IsMine)
        {
            MonoBehaviour[] scripts =  GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour item in scripts)
            {
                if (item is PhotonView) { continue; }
                if (item is PhotonTransformViewClassic){ continue; }
                if (item is PlayerStats){ continue; }

                if (item != this)
                {
                    Destroy(item);
                }                
            }

            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<Camera>().enabled = false;
        }
    }
}
