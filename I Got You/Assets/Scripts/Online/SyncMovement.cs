using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SyncMovement : MonoBehaviourPun, IPunObservable
{
    private Vector3 syncPos;
    private Vector3 syncRot;

    [SerializeField] private float lerpPosSpeed = 5;
    [SerializeField] private float lerpRotSpeed = 5;
    [SerializeField] private bool checkMasterClient = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(new Vector3(ReturnSingleDecimalFloat(transform.position.x), ReturnSingleDecimalFloat(transform.position.y), ReturnSingleDecimalFloat(transform.position.z)));
            stream.SendNext(ReturnSingleDecimalFloat(transform.localEulerAngles.y));
        }
        else if(stream.IsReading)
        {
            syncPos = (Vector3)stream.ReceiveNext();
            syncRot = new Vector3(transform.localEulerAngles.x, (float)stream.ReceiveNext(), transform.localEulerAngles.z);
        }
    }

    private float ReturnSingleDecimalFloat(float value)
    {
        return Mathf.Round(value * 10) * 0.1f;
    }

    private void Start()
    {
        if (!PhotonFunctionHandler.IsPlayerOnline())
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if ((!photonView.IsMine && !checkMasterClient) || (checkMasterClient && !PhotonNetwork.IsMasterClient))
        {
            transform.position = Vector3.Lerp(transform.position, syncPos, lerpPosSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(syncRot), lerpRotSpeed * Time.deltaTime);
        }
    }
}
