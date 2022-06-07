using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteNoRotation : MonoBehaviour
{
    private Quaternion startingRot;
    private PlayerStats playerStats;
    private Vector3 startingPos;
    [SerializeField] private bool onlyForDeath = false;

    // Start is called before the first frame update
    void Start()
    {
        startingRot = transform.rotation;
        startingPos = transform.position;
        playerStats = GetComponentInParent<PlayerStats>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = startingRot;
        transform.position = playerStats.transform.position + new Vector3(0, startingPos.y, 0);
    }
}
