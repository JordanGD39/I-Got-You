using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SyncMovement : MonoBehaviourPun, IPunObservable
{
    private Vector3 syncPos_Vector3;
    private Vector3 syncRot_Vector3;

    [SerializeField] private float lerpPosSpeed_float = 5;
    [SerializeField] private float lerpRotSpeed_float = 5;
    [SerializeField] private float distanceToTeleport_float = 3;
    [SerializeField] private bool checkMasterClient_bool = false;
    [SerializeField] private GameObject model_GameObject;

    public void OnPhotonSerializeView_void(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(new Vector3(ReturnSingleDecimalFloat_float(transform.position.x), ReturnSingleDecimalFloat_float(transform.position.y), ReturnSingleDecimalFloat_float(transform.position.z)));
            stream.SendNext(ReturnSingleDecimalFloat_float(transform.localEulerAngles.y));
        }
        else if(stream.IsReading)
        {
            syncPos_Vector3 = (Vector3)stream.ReceiveNext();
            syncRot_Vector3 = new Vector3(transform.localEulerAngles.x, (float)stream.ReceiveNext(), transform.localEulerAngles.z);
        }
    }

    private float ReturnSingleDecimalFloat_float(float value)
    {
        return Mathf.Round(value * 10) * 0.1f;
    }

    private void Start_void()
    {
        if (!PhotonNetwork.IsConnected)
        {
            if (model_GameObject != null)
            {
                model_GameObject.SetActive(true);
            }
            
            Destroy(this);
            return;
        }

        if (model_GameObject == null)
        {
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            model_GameObject.SetActive(false);
            transform.position = Vector3.zero;
            StartCoroutine(nameof(TeleportToSync_IEnumerator));
        }
    }

    private void Update_void()
    {
        if ((!photonView.IsMine && !checkMasterClient_bool) || (checkMasterClient_bool && !PhotonNetwork.IsMasterClient))
        {
            if (Vector3.Distance(transform.position, syncPos_Vector3) > distanceToTeleport_float)
            {
                transform.position = syncPos_Vector3;
            }
            transform.position = Vector3.Lerp(transform.position, syncPos_Vector3, lerpPosSpeed_float * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(syncRot_Vector3), lerpRotSpeed_float * Time.deltaTime);
        }
    }

    private IEnumerator TeleportToSync_IEnumerator()
    {
        while (transform.position == Vector3.zero)
        {
            yield return null;
        }

        transform.position = syncPos_Vector3;
        transform.rotation = Quaternion.Euler(syncRot_Vector3);
        model_GameObject.SetActive(true);
    }
}
