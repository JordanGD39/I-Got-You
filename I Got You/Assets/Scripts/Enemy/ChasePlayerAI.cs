using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ChasePlayerAI : MonoBehaviour
{
    private AttackAnimationHandler attackAnimationHandler;
    private EnemyRoam enemyRoam;
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private NavMeshAgent agent;
    private NavMeshObstacle navMeshObstacle;
    private GameObject navMeshObstacleObject;
    [SerializeField] private Transform target;
    public Transform TargetPlayer { get { return target; } }
    [SerializeField] private float distanceToAttack = 2;
    [SerializeField] private float distanceToStop = 1.25f;
    [SerializeField] private float turnSpeed = 5;
    [SerializeField] private float fadeDelay = 0.1f;
    [SerializeField] private float fadeTime = 3;
    [SerializeField] private Material normalMat;
    [SerializeField] private Material fadeMat;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private bool fadedIn = false;
    [SerializeField] private bool animsDone = false;
    private Animator anim;
    private Vector3 previousAttackingSpot = Vector3.zero;

    private bool attacking = false;
    private int startingAvoidancePriority = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (skinnedMeshRenderer != null)
        {
            StartFadeIn();
        }
        else
        {
            fadedIn = true;
        }

        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            return;
        }

        enemyRoam = GetComponentInChildren<EnemyRoam>();
        enemyManager = FindObjectOfType<EnemyManager>();
        anim = GetComponentInChildren<Animator>();
        playerManager = FindObjectOfType<PlayerManager>();
        
        agent = GetComponent<NavMeshAgent>();
        startingAvoidancePriority = agent.avoidancePriority;
    }

    public void StartFadeIn()
    {
        skinnedMeshRenderer.material = normalMat;
        StartCoroutine(nameof(FadeInEnemy));
    }

    private IEnumerator FadeInEnemy()
    {
        skinnedMeshRenderer.material = fadeMat;

        float startingTime = Time.time;
        float frac = 0;

        while (frac < 1)
        {
            frac = (Time.time - startingTime) / fadeTime;

            Color color = skinnedMeshRenderer.material.color;

            color.a = Mathf.Lerp(0, 1, frac);

            skinnedMeshRenderer.material.color = color;

            yield return null;
        }

        skinnedMeshRenderer.material = normalMat;
        fadedIn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected || !agent.isOnNavMesh || !fadedIn)
        {
            return;
        }

        if (skinnedMeshRenderer != null && skinnedMeshRenderer.material != normalMat)
        {
            skinnedMeshRenderer.material = normalMat;
        }

        if (playerManager.Players.Count == 0)
        {
            return;
        }

        if (enemyManager.EnemiesTarget == null)
        {
            float closestDist = Mathf.Infinity;

            foreach (PlayerStats player in playerManager.Players)
            {
                if (player == null)
                {
                    continue;
                }

                float dist = Vector3.Distance(player.transform.position, transform.position);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = player.transform;
                }
            }
        }
        else
        {
            target = enemyManager.EnemiesTarget;
        }

        if (target != null && agent.enabled && enemyRoam == null)
        {
            agent.SetDestination(target.position);
        }

        if (animsDone)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(target.position.x, 0, target.position.z));

        agent.updateRotation = distance > distanceToAttack;

        if (distance <= distanceToAttack)
        {
            FaceTarget();
        }
        
        anim.SetBool("Attacking", distance < distanceToAttack);

        if (distance < distanceToStop)
        {
            if (agent.avoidancePriority > 10)
            {
                previousAttackingSpot = transform.position;
            }

            agent.velocity = Vector3.zero;
            agent.avoidancePriority = 10;

            agent.Warp(previousAttackingSpot);

            previousAttackingSpot = transform.position;
        }
        else if(agent.avoidancePriority != startingAvoidancePriority)
        {
            agent.avoidancePriority = startingAvoidancePriority;
        }
    }

    public void FaceTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
    }
}
