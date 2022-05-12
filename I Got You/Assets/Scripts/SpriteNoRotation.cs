using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteNoRotation : MonoBehaviour
{
    private Quaternion startingRot;

    // Start is called before the first frame update
    void Start()
    {
        startingRot = transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = startingRot;
    }
}
