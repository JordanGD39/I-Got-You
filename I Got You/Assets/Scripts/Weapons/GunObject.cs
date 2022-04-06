using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "ScriptableObjects/GunObject", order = 1)]
public class GunObject : ScriptableObject
{
    [SerializeField] private int damage = 1;
    public int Damage { get { return damage; } }
    [SerializeField] private float shootDelay = 0.5f;
    public float ShootDelay { get { return shootDelay; } }
    [SerializeField] private float burstDelay = 0.1f;
    public float BurstDelay { get { return burstDelay; } }
    [SerializeField] private float reloadSpeed = 2;
    public float ReloadSpeed { get { return reloadSpeed; } }
    [SerializeField] private int clipCount = 8;
    public int ClipCount { get { return clipCount; } }
    [SerializeField] private int maxAmmoCount = 80;
    public int MaxAmmoCount { get { return maxAmmoCount; } }
    [SerializeField] private float headShotMultiplier = 1.4f;
    public float HeadShotMultiplier { get { return headShotMultiplier; } }
    [SerializeField] private float maxRange = 1500;
    public float MaxRange { get { return maxRange; } }
    [SerializeField] private int bulletCount = 1;
    public int BulletCount { get { return bulletCount; } }
    [SerializeField] private float spreadAngle = 0.1f;
    public float SpreadAngle { get { return spreadAngle; } }

    public enum ShootTypes { MANUAL, AUTO, BURST}
    [SerializeField] private ShootTypes shootType;
    public ShootTypes ShootType { get { return shootType; } }
}
