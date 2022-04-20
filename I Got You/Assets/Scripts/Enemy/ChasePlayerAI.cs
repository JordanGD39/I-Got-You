using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChasePlayerAI : MonoBehaviour
{
    private AttackAnimationHandler attackAnimationHandler;
    private PlayerManager playerManager;
    private NavMeshAgent agent;
    private NavMeshObstacle navMeshObstacle;
    private GameObject navMeshObstacleObject;
    [SerializeField] private Transform target;
    public Transform TargetPlayer { get { return target; } }
    [SerializeField] private float distanceToAttack = 2;
    [SerializeField] private float distanceToStop = 1.25f;
    [SerializeField] private float turnSpeed = 5;
    private Animator anim;

    private bool attacking = false;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        agent = GetComponent<NavMeshAgent>();
        
        anim = GetComponentInChildren<Animator>();
        GetComponentInChildren<AttackAnimationHandler>().OnAttackMiss += AttackStopped;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerManager.Players.Count == 0)
        {
            return;
        }

        float closestDist = Mathf.Infinity;        

        foreach (PlayerStats player in playerManager.Players)
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                target = player.transform;
            }
        }

        if (target != null && agent.enabled)
        {
            agent.SetDestination(target.position);
        }        

        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(target.position.x, 0, target.position.z));

        if (distance <= agent.stoppingDistance)
        {
            FaceTarget();
        }


        if (!attacking && distance < distanceToAttack)
        {
            anim.SetTrigger("Attack");
            attacking = true;
        }

        if (distance < distanceToStop)
        {
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }
        else
        {
            agent.enabled = true;
        }
    }

    public void FaceTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
    }

    public void AttackStopped()
    {
        attacking = false;
    }
}
