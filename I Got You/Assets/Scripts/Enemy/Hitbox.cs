using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public delegate void HitBoxCollided(Collider other);
    public HitBoxCollided OnHitBoxCollided;

    private void OnTriggerEnter(Collider other)
    {
        OnHitBoxCollided?.Invoke(other);
    }
}
