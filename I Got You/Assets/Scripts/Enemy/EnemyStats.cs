using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 20;

    // Start is called before the first frame update
    void Start()
    {
        List<Hitbox> hitboxes = new List<Hitbox>();

        hitboxes.AddRange(GetComponentsInChildren<Hitbox>());

        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.OnHitBoxCollided += DamagePlayer;
            hitbox.gameObject.SetActive(false);
        }
    }

    public void Damage(int dmg)
    {
        Debug.Log("Damage: " + dmg);
        health -= dmg;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void DamagePlayer(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerCol"))
        {
            other.GetComponentInParent<PlayerStats>().Damage(damage);
        }
    }
}
