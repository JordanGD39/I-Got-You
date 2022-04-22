using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHolder : MonoBehaviour
{
    [SerializeField] private GunObject owningGun;
    public GunObject OwningGun { get { return owningGun; } }

    public Animator GunAnim { get; private set; }
    public ParticleSystem MuzzleFlash { get; private set; }

    public delegate void GunPutAway();
    public GunPutAway OnGunPutAway;

    public delegate void GunCanShoot();
    public GunPutAway OnGunCanShoot;

    private void Start()
    {
        if (GunAnim == null)
        {
            GunAnim = GetComponent<Animator>();
            MuzzleFlash = GetComponentInChildren<ParticleSystem>();
        }
    }

    public void GunIsPutAway()
    {
        OnGunPutAway?.Invoke();
    }

    public void GunIsAbleToShoot()
    {
        OnGunCanShoot?.Invoke();
    }
}
