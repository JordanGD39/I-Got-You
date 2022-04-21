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
        meshCollider.enabled = true;

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

        foreach (RagdollJointComponents joint in jointComponents)
        {
            if (!active)
            {
                joint.rb.velocity = Vector3.zero;
                joint.rb.transform.localPosition = joint.jointPosition;
                joint.rb.transform.localRotation = joint.jointRotation;
                skinnedMeshRenderer.material = originalMat;
                meshCollider.enabled = true;
                playerCollider.SetActive(true);
                StopAllCoroutines();
            }
            else
            {
                playerCollider.SetActive(false);
                meshCollider.enabled = false;

                StartCoroutine(nameof(FadeOutEnemy));
            }

            joint.rb.isKinematic = !active;            
        }

        anim.enabled = !active;
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
    public Vector3 jointPosition;
    public Quaternion jointRotation;
}
