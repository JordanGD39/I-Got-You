using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteNoRotation : MonoBehaviour
{
    private Quaternion startingRot;
    private PlayerStats playerStats;
    private Vector3 oldPos;

    // Start is called before the first frame update
    void Start()
    {
        startingRot = transform.rotation;
        playerStats = GetComponentInParent<PlayerStats>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = startingRot;

        if (playerStats == null)
        {
            return;
        }

        if (playerStats.IsDown || playerStats.IsDead)
        {
            transform.position = oldPos;
            return;
        }

        oldPos = transform.position;
    }
}
