using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private List<RagdollJointComponents> jointComponents = new List<RagdollJointComponents>();
    [SerializeField] private Rigidbody mainRigidBody;
    [SerializeField] private float upwardsModifier = 0.5f;
    [SerializeField] private float normalModifier = 3f;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private GameObject[] collidersToDisable;
    [SerializeField] private GameObject playerCollider;
    [SerializeField] private Material fadeMat;
    [SerializeField] private float fadeDelay = 3;
    [SerializeField] private float fadeTime = 1;
    private Material uniqueFadeMat;
    private Material originalMat;
    private Animator anim;
    private bool busy = true;

    // Start is called before the first frame update
    void Start()
    {
        if (meshCollider != null)
        {
            meshCollider.enabled = true;
        }
        else
        {
            foreach (GameObject col in collidersToDisable)
            {
                col.SetActive(true);
            }
        }

        if (anim != null)
        {
            skinnedMeshRenderer.material = originalMat;
            SetRagdollActive(false);
            return;
        }

        anim = GetComponent<Animator>();

        originalMat = skinnedMeshRenderer.sharedMaterial;

        foreach (Rigidbody joint in GetComponentsInChildren<Rigidbody>(true))
        {
            RagdollJointComponents ragdollJointComponents = new RagdollJointComponents();
            ragdollJointComponents.rb = joint;
            ragdollJointComponents.colliders = GetComponents<Collider>();
            ragdollJointComponents.jointPosition = joint.transform.localPosition;
            ragdollJointComponents.jointRotation = joint.transform.localRotation;

            jointComponents.Add(ragdollJointComponents);
        }

        busy = false;

        SetRagdollActive(false);        
    }

    public void SetRagdollActive(bool active)
    {
        if (busy)
        {
            return;
        }

        anim.enabled = !active;

        foreach (RagdollJointComponents joint in jointComponents)
        {
            if (!active)
            {
                joint.rb.velocity = Vector3.zero;
                joint.rb.transform.localPosition = joint.jointPosition;
                joint.rb.transform.localRotation = joint.jointRotation;

                foreach (Collider item in joint.colliders)
                {
                    item.enabled = false;
                }

                skinnedMeshRenderer.material = originalMat;

                if (meshCollider != null)
                {
                    meshCollider.enabled = true;
                }
                else
                {
                    foreach (GameObject col in collidersToDisable)
                    {
                        col.SetActive(true);
                    }
                }
                
                playerCollider.SetActive(true);
                StopAllCoroutines();
            }
            else
            {
                playerCollider.SetActive(false);

                if (meshCollider != null)
                {
                    meshCollider.enabled = false;
                }
                else
                {
                    foreach (GameObject col in collidersToDisable)
                    {
                        col.SetActive(false);
                    }
                }

                foreach (Collider item in joint.colliders)
                {
                    item.enabled = true;
                }

                StartCoroutine(nameof(FadeOutEnemy));
            }

            //joint.rb.collisionDetectionMode = active ? CollisionDetectionMode.Continuous : CollisionDetectionMode.ContinuousSpeculative;
            joint.rb.isKinematic = !active;
        }
    }

    public void ApplyForceToMainRigidBody(Vector3 dir, float force)
    {
        mainRigidBody.AddForce(force * upwardsModifier * Vector3.up);
        mainRigidBody.AddForce(force * normalModifier * dir, ForceMode.Impulse);
    }

    private IEnumerator FadeOutEnemy()
    {
        yield return new WaitForSeconds(fadeDelay);

        uniqueFadeMat = new Material(fadeMat);
        skinnedMeshRenderer.material = uniqueFadeMat;

        foreach (RagdollJointComponents joint in jointComponents)
        {
            joint.rb.isKinematic = true;
        }

        float startingTime = Time.time;
        float val = uniqueFadeMat.color.a;
        float frac = 0;

        while (frac < 1)
        {
            frac = (Time.time - startingTime) / fadeTime;

            Color color = uniqueFadeMat.color;
            
            color.a = Mathf.Lerp(val, 0, frac);

            uniqueFadeMat.color = color;

            yield return null;
        }

        transform.root.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class RagdollJointComponents
{
    public Rigidbody rb;
    public Collider[] colliders;
    public Vector3 jointPosition;
    public Quaternion jointRotation;
}
