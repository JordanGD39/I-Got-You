using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRoam : MonoBehaviour
{
    private Collider ground;
    private EnemyPlayerSpotter enemyPlayerSpotter;
    private NavMeshAgent agent;

    private bool alreadyInvoking = false;
    private Vector3 targetPos;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private float delayRoamSearchSeconds = 1f;
    [SerializeField] private float minimumDistance = 40;
    private Vector3 boundsMin;
    private Vector3 boundsMax;

    // Start is called before the first frame update
    void Start()
    {
        enemyPlayerSpotter = GetComponent<EnemyPlayerSpotter>();
        agent = GetComponent<NavMeshAgent>();
        Invoke(nameof(ChooseRandomLocation), delayRoamSearchSeconds);
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyPlayerSpotter.PlayerSpotted)
        {
            alreadyInvoking = false;
            agent.SetDestination(enemyPlayerSpotter.Target.position);
        }
        else
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                alreadyInvoking = false;
            }

            if (!alreadyInvoking)
            {
                Invoke(nameof(ChooseRandomLocation), delayRoamSearchSeconds);
            }
        }        
    }

    private void ChooseRandomLocation()
    {
        if (alreadyInvoking)
        {
            return;
        }

        alreadyInvoking = true;

        RaycastHit groundHit;

        int layer = LayerMask.GetMask("Ground");
        float x = 0;
        float z = 0;
        if (Physics.Raycast(transform.position, Vector3.down, out groundHit, 100, layer))
        {
            ground = groundHit.collider;

            x = ground.bounds.extents.x;
            z = ground.bounds.extents.z;
        }
        else
        {
            if (ground == null)
            {
                return;
            }
        }

        boundsMin = new Vector3(ground.transform.position.x - x, 0, ground.transform.position.z - z);
        boundsMax = new Vector3(ground.transform.position.x + x, 0, ground.transform.position.z + z);

        Vector3 maybeTargetPos = new Vector3(ground.transform.position.x + Random.Range(-x + 2, x - 2), 0, ground.transform.position.z + Random.Range(-z + 2, z - 2));

        if (enemyPlayerSpotter.PlayerSpotted)
        {
            return;
        }

        if (Vector3.Distance(maybeTargetPos, transform.position) < minimumDistance)
        {
            Debug.Log("Not far enough");
            alreadyInvoking = false;
            return;
        }

        RaycastHit hit;

        int layerCheck = LayerMask.GetMask("Default") | LayerMask.GetMask("IgnoreNavmesh");

        if (Physics.Raycast(maybeTargetPos, Vector3.up, out hit, transform.localScale.y, layerCheck))
        {
            if (hit.collider.gameObject.CompareTag("ExcludeGround"))
            {
                Debug.Log("Exclusion zone");
                alreadyInvoking = false;
                return;
            }

            if (hit.collider != null)
            {
                alreadyInvoking = false;
                Debug.Log("Something is there " + hit.collider.gameObject);
                return;
            }
        }
        else
        {
            targetPos = maybeTargetPos;
            maybeTargetPos.y += checkRadius * 2;

            if (!Physics.CheckSphere(maybeTargetPos, checkRadius, layerCheck))
            {
                maybeTargetPos.y -= checkRadius * 2;
                agent.SetDestination(maybeTargetPos);
            }
            else
            {
                alreadyInvoking = false;
                Debug.Log("Someting is nearby the target pos");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetPos, checkRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(boundsMin, checkRadius);
        Gizmos.DrawWireSphere(boundsMax, checkRadius);
    }
}
