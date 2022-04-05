using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField]
    private MeleeWeapon currentWeapon;
    public MeleeWeapon CurrentWeapon { get { return currentWeapon; } }
    [SerializeField]
    private int animIndex = 0;
    private bool attacking = false;

    [SerializeField]
    private Animator weaponAnim;

    private void Start()
    {
        weaponAnim.runtimeAnimatorController = currentWeapon.AttackAnim;
    }

    // Update is called once per frame
    void Update()
    {
        if (!attacking && Input.GetMouseButtonDown(0))
        {
            weaponAnim.SetInteger("comboCounter", animIndex);
            weaponAnim.SetBool("attack", true);
            attacking = true;
        }
    }

    public void AttackHit()
    {
        if (!attacking)
        {
            return;
        }
        weaponAnim.SetBool("attack", false);

        attacking = false;

        animIndex++;

        if (animIndex >= currentWeapon.MaxCombo + 1)
        {
            animIndex = 0;
        }
    }

    public void AttackMissed()
    {
        weaponAnim.SetBool("attack", false);
        attacking = false;
        animIndex = 0;
    }
}
