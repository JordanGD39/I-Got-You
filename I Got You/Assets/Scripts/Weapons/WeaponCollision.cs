using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponCollision : MonoBehaviour
{
    private PlayerCombat playerCombat;

    private bool hit = false;


    // Start is called before the first frame update
    void Start()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
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
