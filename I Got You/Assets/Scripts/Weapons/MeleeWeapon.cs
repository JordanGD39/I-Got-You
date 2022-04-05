using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee", menuName = "ScriptableObjects/MeleeWeapon", order = 1)]
public class MeleeWeapon : ScriptableObject
{
    [SerializeField]
    private GameObject weaponModel;
    public GameObject WeaponModel { get { return weaponModel; } }
    [SerializeField]
    private RuntimeAnimatorController attackAnim;
    public RuntimeAnimatorController AttackAnim { get { return attackAnim; } }
    [SerializeField] private int maxCombo = 3;
    public int MaxCombo { get { return maxCombo; } }
    [SerializeField]
    private int damage = 1;
    public int Damage { get { return damage; } }
    [SerializeField]
    private float attackSpeed = 1;
    public float AttackSpeed { get { return attackSpeed; } }
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary };
    [SerializeField]
    private Rarity weaponRarity;
    public Rarity WeaponRarity { get { return weaponRarity; } }
    public enum DamageTypes { Slashing, Piercing, Blugeoning };
    [SerializeField]
    private DamageTypes damageType;
    public DamageTypes DamageType { get { return damageType; } }
    public enum MagicEffect { None, Acid, Cold, Explosive, Fire, Force, Lightning, Necrotic, Poison, Psychic, Radiant, Thunder };
    [SerializeField]
    private MagicEffect effect;
    public MagicEffect Effect { get { return effect; } }
}
