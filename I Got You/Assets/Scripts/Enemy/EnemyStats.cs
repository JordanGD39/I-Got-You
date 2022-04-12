using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyStats : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private int startingHealth = 50;
    public int StartingHealth { get { return startingHealth; } }
    [SerializeField] private int health = 100;
    public int Health { get { return health; } set { health = value; } }
    [SerializeField] private int damage = 20;
    public int ListIndex { get; set; } = 0;

    private EnemyManager enemyManager;
    private PlayerManager playerManager;

    public delegate void EnemyDied(GameObject enemy, int index);
    public EnemyDied OnEnemyDied;

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            enemyManager.StatsOfAllEnemies.Add(transform.GetChild(0), this);
        }        
    }

    // Start is called before the first frame update
    void Start()
    {
        health = startingHealth;

        playerManager = FindObjectOfType<PlayerManager>();

        List<Hitbox> hitboxes = new List<Hitbox>();

        hitboxes.AddRange(GetComponentsInChildren<Hitbox>());

        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.OnHitBoxCollided += DamagePlayer;
            hitbox.gameObject.SetActive(false);
        }

        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            gameObject.SetActive(false);
        }
    }

    public void Damage(int dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            OnEnemyDied?.Invoke(gameObject, ListIndex);
            CallSyncHealth();
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                StopCoroutine(nameof(WaitForHealthUpdate));
                StartCoroutine(nameof(WaitForHealthUpdate));
            }
        }
    }

    private IEnumerator WaitForHealthUpdate()
    {
        yield return new WaitForSeconds(0.1f);

        CallSyncHealth();
    }

    public void CallSyncHealth()
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncHealthRPC", RpcTarget.Others, health);
        }
    }

    [PunRPC]
    void SyncHealthRPC(int hp)
    {
        health = hp;

        if (health <= 0)
        {
            OnEnemyDied?.Invoke(gameObject, ListIndex);
            gameObject.SetActive(false);
        }
    }

    private void DamagePlayer(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerCol"))
        {
            playerManager.StatsOfAllPlayers[other].Damage(damage);
        }
    }
}
