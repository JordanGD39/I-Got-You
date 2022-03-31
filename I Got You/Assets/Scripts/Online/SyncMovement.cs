using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SyncMovement : MonoBehaviourPun, IPunObservable
{
    private Vector3 syncPos;
    private Quaternion syncRot;

    [SerializeField] private float lerpPosSpeed = 5;
    [SerializeField] private float lerpRotSpeed = 5;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(new Vector3(ReturnSingleDecimalFloat(transform.position.x), ReturnSingleDecimalFloat(transform.position.y), ReturnSingleDecimalFloat(transform.position.z)));
            stream.SendNext(ReturnSingleDecimalFloat(transform.localRotation.y));
        }
        else if(stream.IsReading)
        {
            syncPos = (Vector3)stream.ReceiveNext();
            syncRot = new Quaternion(transform.localRotation.x, (float)stream.ReceiveNext(), transform.localRotation.z, transform.localRotation.w);
        }
    }

    private float ReturnSingleDecimalFloat(float value)
    {
        return Mathf.Round(value * 10) * 0.1f;
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, syncPos, lerpPosSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, syncRot, lerpRotSpeed * Time.deltaTime);
        }
    }
}
