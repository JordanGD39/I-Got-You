using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChasePlayerAI : MonoBehaviour
{
    private AttackAnimationHandler attackAnimationHandler_AttackAnimationHandler;
    private PlayerManager playerManager_PlayerManager;
    private NavMeshAgent agent_NavMeshAgent;
    [SerializeField] private Transform target_Transform;
    public Transform TargetPlayer_Transform { get { return target_Transform; } }
    [SerializeField] private float distanceToAttack_float = 2;
    [SerializeField] private float distanceToStop_float = 1.25f;
    [SerializeField] private float turnSpeed_float = 5;
    private Animator anim_Animator;

    private bool attacking_bool = false;

    // Start is called before the first frame update
    void Start_void()
    {
        playerManager_PlayerManager = FindObjectOfType<PlayerManager>();
        agent_NavMeshAgent = GetComponent<NavMeshAgent>();
        
        anim_Animator = GetComponentInChildren<Animator>();
        GetComponentInChildren<AttackAnimationHandler>().OnAttackMiss_AttackMiss += AttackStopped_void;
    }

    // Update is called once per frame
    void Update_void()
    {
        if (playerManager_PlayerManager.Players.Count == 0)
        {
            return;
        }

        float closestDist = Mathf.Infinity;        

        foreach (PlayerStats player in playerManager_PlayerManager.Players)
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                target_Transform = player.transform;
            }
        }

        if (target_Transform != null && agent_NavMeshAgent.enabled)
        {
            agent_NavMeshAgent.SetDestination(target_Transform.position);
        }        

        float distance = Vector3.Distance(transform.position, target_Transform.position);

        if (distance <= agent_NavMeshAgent.stoppingDistance)
        {
            FaceTarget_void();
        }


        if (!attacking_bool && distance < distanceToAttack_float)
        {
            anim_Animator.SetTrigger("Attack");
            attacking_bool = true;
        }

        if (distance < distanceToStop_float)
        {
            agent_NavMeshAgent.velocity = Vector3.zero;
            agent_NavMeshAgent.enabled = false;
        }
        else
        {
            agent_NavMeshAgent.enabled = true;
        }
    }

    public void FaceTarget_void()
    {
        Vector3 lookPos = target_Transform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed_float * Time.deltaTime);
    }

    public void AttackStopped_void()
    {
        attacking_bool = false;
    }
}
