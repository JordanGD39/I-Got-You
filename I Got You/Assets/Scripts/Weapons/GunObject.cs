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
    [SerializeField] private float reloadSpeed = 2;
    public float ReloadSpeed { get { return reloadSpeed; } }
    [SerializeField] private int clipCount = 8;
    public int ClipCount { get { return clipCount; } }
    [SerializeField] private int maxAmmoCount = 80;
    public int MaxAmmoCount { get { return maxAmmoCount; } }
    [SerializeField] private float headShotMultiplier = 1.4f;
    public float HeadShotMultiplier { get { return headShotMultiplier; } }
    [SerializeField] private float closeDamageMultiplier = 1;
    public float CloseDamageMultiplier { get { return closeDamageMultiplier; } }
    [SerializeField] private float farDamageMultiplier = 1;
    public float FarDamageMultiplier { get { return farDamageMultiplier; } }
    [SerializeField] private float maxRange = 1500;
    public float MaxRange { get { return maxRange; } }

    public enum ShootTypes { MANUAL, AUTO, BURST}
    [SerializeField] private ShootTypes shootType;
    public ShootTypes ShootType { get { return shootType; } }
}
