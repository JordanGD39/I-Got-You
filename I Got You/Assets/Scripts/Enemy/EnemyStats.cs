using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyStats : MonoBehaviourPun
{
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 20;

    [SerializeField] private List<MeleeWeapon.DamageTypes> resitances = new List<MeleeWeapon.DamageTypes>();
    [SerializeField] private List<MeleeWeapon.DamageTypes> weaknesses = new List<MeleeWeapon.DamageTypes>();

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

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncHealthRPC", RpcTarget.Others, health);
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    void SyncHealthRPC(int hp)
    {
        health = hp;

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

    public void MeleeDamage(MeleeWeapon meleeWeapon)
    {
        int damage = meleeWeapon.Damage;

        if (resitances.Count > 0 && resitances.Contains(meleeWeapon.DamageType))
        {
            damage /= 2;
        }
        else if (weaknesses.Count > 0 && weaknesses.Contains(meleeWeapon.DamageType))
        {
            damage *= 2;
        }

        health -= damage;
    }
}
