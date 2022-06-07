using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteNoRotation : MonoBehaviour
{
    private Quaternion startingRot;
    private PlayerStats playerStats;
    private Vector3 oldPos;
    [SerializeField] private bool onlyForDeath = false;

    // Start is called before the first frame update
    void Start()
    {
        startingRot = transform.rotation;
        playerStats = GetComponentInParent<PlayerStats>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!onlyForDeath)
        {
            transform.rotation = startingRot;
        }        
        else
        {
            transform.rotation = new Quaternion(startingRot.x, transform.rotation.y, startingRot.z, transform.rotation.w);
        }

        if (playerStats == null)
        {
            return;
        }

        if (playerStats.IsDown || playerStats.IsDead)
        {
            transform.position = playerStats.transform.position + new Vector3(0, oldPos.y, 0);
            return;
        }

        oldPos = transform.position;
    }
}
