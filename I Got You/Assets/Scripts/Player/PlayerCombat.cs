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
            weaponAnim.SetTrigger("attack");
            attacking = true;
        }
    }

    public void AttackHit()
    {
        weaponAnim.ResetTrigger("attack");
        animIndex++;

        if (animIndex >= currentWeapon.MaxCombo)
        {
            animIndex = 0;
        }

        attacking = false;
    }

    public void AttackMissed()
    {
        weaponAnim.ResetTrigger("attack");
        attacking = false;
        animIndex = 0;
    }
}
