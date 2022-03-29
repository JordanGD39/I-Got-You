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

        agent.SetDestination(target.position);

        if (!attacking && Vector3.Distance(transform.position, target.position) < distanceToAttack)
        {
            anim.SetTrigger("Attack");
            attacking = true;
        }
    }

    public void AttackStopped()
    {
        attacking = false;
    }
}
