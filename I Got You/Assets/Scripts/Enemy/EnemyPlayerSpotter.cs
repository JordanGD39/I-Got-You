using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPlayerSpotter : MonoBehaviour
{
    [SerializeField] private Transform eyes;
    [SerializeField] private Transform target;
    [SerializeField] private float viewDistance = 3;
    [SerializeField] private float fov = 105;
    [SerializeField] private float forgetPlayerTime = 3;
    [SerializeField] private LayerMask layerDetectPlayer;
    [SerializeField] private float speedWhenSeeing = 10;
    [SerializeField] private float rotSpeedWhenSeeing = 100;
    [SerializeField] private float standStillTime = 1;
    private float baseSpeed = 10;
    private float baseRotSpeed = 210;
    private bool playerSpotted = false;
    private float sightTimer = 0;
    private bool lookingAtTarget = false;

    private ChasePlayerAI chaseAI;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        chaseAI = GetComponent<ChasePlayerAI>();
        agent = GetComponent<NavMeshAgent>();

        baseSpeed = agent.speed;
        baseRotSpeed = agent.angularSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInSightCheck();
    }

    private void PlayerInSightCheck()
    {
        bool playerSpottedNow = false;

        if (Vector3.Distance(chaseAI.TargetPlayer.position, transform.position) < viewDistance)
        {
            Vector3 dirToPlayer = (chaseAI.TargetPlayer.position - eyes.position).normalized;

            if (Vector3.Angle(eyes.forward, dirToPlayer) < fov / 2)
            {
                //Debug.Log("Player in field of view");
                RaycastHit hit;

                if (Physics.Raycast(eyes.position, dirToPlayer, out hit, viewDistance, layerDetectPlayer))
                {
                    if (hit.collider.gameObject.CompareTag("PlayerCol"))
                    {
                        playerSpottedNow = true;
                    }
                }
            }
        }

        if (playerSpottedNow)
        {
            if (!lookingAtTarget)
            {
                lookingAtTarget = true;
                playerSpotted = true;

                agent.speed = 0;

                Invoke(nameof(ChangeAgentSpeed), standStillTime);
                return;
            }
            else if(agent.speed == 0)
            {
                chaseAI.FaceTarget();
            }

            return;
        }        

        Color color = Color.red;

        if (playerSpottedNow)
        {
            color = Color.green;
        }

        Debug.DrawRay(eyes.position, transform.forward, color);

        if (!playerSpotted)
        {
            return;
        }


        if (sightTimer < forgetPlayerTime)
        {
            if (playerSpotted)
            {
                sightTimer += Time.deltaTime;
            }            
        }
        else
        {
            sightTimer = 0;
            agent.speed = baseSpeed;
            agent.angularSpeed = baseRotSpeed;
            playerSpotted = false;
            lookingAtTarget = false;
        }
    }

    private void ChangeAgentSpeed()
    {
        agent.speed = speedWhenSeeing;
        agent.angularSpeed = rotSpeedWhenSeeing;
        sightTimer = 0;
    }
}
