using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class EnemyStats : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private int startingHealth = 50;
    public int StartingHealth { get { return startingHealth; } }
    [SerializeField] private int health = 100;
    public int Health { get { return health; } set { health = value; } }
    [SerializeField] private int damage = 20;
    [SerializeField] private bool invincible = false;
    public int ListIndex { get; set; } = -1;

    private EnemyManager enemyManager;
    private PlayerManager playerManager;
    private RagdollController ragdollController;
    private NavMeshAgent agent;
    private List<MonoBehaviour> componentsToWork = new List<MonoBehaviour>();
    private List<Hitbox> hitboxes = new List<Hitbox>();

    public delegate void EnemyDied(GameObject enemy, int index);
    public EnemyDied OnEnemyDied;
    private bool dead = false;

    private SyncMovement syncMovement;

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            enemyManager.StatsOfAllEnemies.Add(transform, this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        dead = false;
        health = startingHealth;
        ragdollController = GetComponentInChildren<RagdollController>(true);

        if (ragdollController != null)
        {
            ragdollController.SetRagdollActive(false);
        }        

        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected && GetComponent<EnemyRoam>() == null)
        {
            gameObject.SetActive(false);
        }

        if (playerManager != null)
        {
            return;
        }

        playerManager = FindObjectOfType<PlayerManager>();

        hitboxes.AddRange(GetComponentsInChildren<Hitbox>(true));

        foreach (MonoBehaviour comp in GetComponents<MonoBehaviour>())
        {
            if (comp is PhotonView)
                continue;
            if (comp is EnemyStats)
                continue;

            componentsToWork.Add(comp);
        }

        syncMovement = GetComponent<SyncMovement>();

        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.OnHitBoxCollided += DamagePlayer;
            hitbox.gameObject.SetActive(false);
        }
    }

    public void Damage(int dmg, Vector3 shootDir)
    {
        if (invincible)
        {
            return;
        }

        health -= dmg;

        if (dead)
        {
            if (ragdollController != null)
            {
                ragdollController.ApplyForceToMainRigidBody(shootDir, dmg);
            }            
            return;
        }

        if (health <= 0)
        {
            CallSyncHealth(dmg, shootDir);
            
            if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
            {
                OnEnemyDied?.Invoke(gameObject, ListIndex);
            }               

            KillEnemy(dmg, shootDir);
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

    private void KillEnemy(int dmg, Vector3 shootDir)
    {
        dead = true;

        agent.velocity = Vector3.zero;
        agent.enabled = false;

        if (syncMovement != null)
        {
            syncMovement.IsSyncing = false;
        }

        if (ragdollController == null)
        {
            gameObject.SetActive(false);
            return;
        }

        photonView.Synchronization = ViewSynchronization.Off;

        foreach (MonoBehaviour item in componentsToWork)
        {
            if (item != null)
            {
                item.enabled = false;
            }            
        }

        foreach (Hitbox item in hitboxes)
        {
            if (item != null)
            {
                item.gameObject.SetActive(false);
            }
        }

        ragdollController.SetRagdollActive(true);
        ragdollController.ApplyForceToMainRigidBody(shootDir, dmg);
    }

    public void CallDisableRagdoll()
    {
        dead = false;

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        agent.enabled = true;

        if (syncMovement == null)
        {
            syncMovement = GetComponent<SyncMovement>();
        }

        photonView.Synchronization = ViewSynchronization.UnreliableOnChange;

        if (syncMovement != null)
        {
            syncMovement.IsSyncing = true;
        }        

        foreach (MonoBehaviour item in componentsToWork)
        {
            if (item != null)
            {
                item.enabled = true;
            }
        }

        if (ragdollController != null)
        {
            ragdollController.SetRagdollActive(false);
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncDisableRagdollOthers", RpcTarget.Others);
        }
    }

    [PunRPC]
    void SyncDisableRagdollOthers()
    {
        CallDisableRagdoll();
    }

    private IEnumerator WaitForHealthUpdate()
    {
        yield return new WaitForSeconds(0.1f);

        CallSyncHealth(0, Vector3.zero);
    }

    public void CallSyncHealth(int dmg, Vector3 shootDir)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (health <= 0)
            {
                photonView.RPC("SyncDeathOthersRPC", RpcTarget.Others, dmg, shootDir);
                return;
            }

            photonView.RPC("SyncHealthRPC", RpcTarget.Others, health);
        }
    }

    [PunRPC]
    void SyncHealthRPC(int hp)
    {
        health = hp;
    }

    [PunRPC]
    void SyncDeathOthersRPC(int dmg, Vector3 shootDir)
    {
        OnEnemyDied?.Invoke(gameObject, ListIndex);
        KillEnemy(dmg, shootDir);
    }

    private void DamagePlayer(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerCol"))
        {
            playerManager.StatsOfAllPlayers[other].Damage(damage);
        }
    }
}
