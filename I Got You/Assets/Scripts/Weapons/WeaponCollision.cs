using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    private PlayerCombat playerCombat;

    private bool hit = false;


    // Start is called before the first frame update
    void Start()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();

        GetComponentInChildren<AttackAnimationHandler>().OnAttackMiss += Missed;
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("EnemyCol"))
        {
            collision.gameObject.GetComponent<EnemyStats>().MeleeDamage(playerCombat.CurrentWeapon);
            playerCombat.AttackHit();
            hit = true;
        }
    }

    public void Missed()
    {
        if (hit)
        {
            hit = false;
            return;
        }

        playerCombat.AttackMissed();
    }
}
