using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChasePlayerAI : MonoBehaviour
{
    private AttackAnimationHandler attackAnimationHandler;
    private PlayerManager playerManager;
    private NavMeshAgent agent;
    [SerializeField] private Transform target;
    [SerializeField] private float distanceToAttack = 50;
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

        if (target != null)
        {
            agent.SetDestination(target.position);
        }        

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            FaceTarget();
        }

        if (!attacking && Vector3.Distance(transform.position, target.position) < distanceToAttack)
        {
            anim.SetTrigger("Attack");
            attacking = true;
        }
    }

    private void FaceTarget()
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
