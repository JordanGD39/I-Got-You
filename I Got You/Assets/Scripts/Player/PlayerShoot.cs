using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private GunObject currentGun;
    [SerializeField] private int currentAmmo = 0;
    [SerializeField] private int currentMaxAmmo = 0;
    [SerializeField] private float distanceForExtraDamage = 20;
    [SerializeField] private float distanceForLowerDamage = 100;
    [SerializeField] private Animator gunAnim;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private LayerMask hitLayer;

    private float timer = 0;
    private bool holdingTrigger = false;
    private bool prevHoldTrigger = false;
    private bool reloading = false;

    private PlayerUI playerUI;

    // Start is called before the first frame update
    void Start()
    {
        currentAmmo = currentGun.ClipCount;
        currentMaxAmmo = currentGun.MaxAmmoCount;

        playerUI = FindObjectOfType<PlayerUI>();
        playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
    }

    // Update is called once per frame
    void Update()
    {
        CheckShoot();
        CheckReload();
    }

    private void CheckShoot()
    {
        if (currentAmmo < 0 || (currentMaxAmmo <= 0 && currentAmmo <= 0))
        {
            return;
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;

            return;
        }

        if (reloading)
        {
            int neededbullets = currentGun.ClipCount - currentAmmo;

            if (neededbullets > currentMaxAmmo)
            {
                neededbullets = currentMaxAmmo;
            }

            currentAmmo += neededbullets;
            currentMaxAmmo -= neededbullets;    
            playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
            reloading = false;
            gunAnim.speed = 1;
        }

        holdingTrigger = Input.GetAxisRaw("Attack") > 0.5f;

        bool attackPressed = Input.GetButtonDown("Attack") || (holdingTrigger && !prevHoldTrigger);
        bool attackHold = Input.GetButton("Attack") || holdingTrigger;

        prevHoldTrigger = holdingTrigger;

        if (currentGun.ShootType == GunObject.ShootTypes.MANUAL && attackPressed || currentGun.ShootType == GunObject.ShootTypes.AUTO && attackHold)
        {
            ShootCurrentGun();
            currentAmmo -= 1;
            playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
            gunAnim.SetTrigger("Shoot");

            if (currentAmmo > 0)
            {
                timer = currentGun.ShootDelay;
            }
            else
            {
                ReloadGun();
            }
        }
    }

    private void ShootCurrentGun()
    {
        muzzleFlash.Play();

        RaycastHit hit;

        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, currentGun.MaxRange, hitLayer))
        {
            Debug.Log(hit.collider.gameObject);

            if (hit.collider.gameObject.CompareTag("EnemyCol"))
            {
                hit.collider.GetComponentInParent<EnemyStats>().Damage(Mathf.RoundToInt((float)currentGun.Damage * CheckHitDistance(hit.point)));
            }
            else if (hit.collider.gameObject.CompareTag("HeadEnemyCol"))
            {
                hit.collider.GetComponentInParent<EnemyStats>().Damage(Mathf.RoundToInt((float)currentGun.Damage * currentGun.HeadShotMultiplier * CheckHitDistance(hit.point)));
            }
        }
    }

    private float CheckHitDistance(Vector3 hitPos)
    {
        if (currentGun.CloseDamageMultiplier == 1)
        {
            return 1;
        }

        float dist = Vector3.Distance(hitPos, transform.position);

        if (dist < distanceForExtraDamage)
        {
            Debug.Log("Close hit! " + dist);
            return currentGun.CloseDamageMultiplier;
        }
        else if(dist > distanceForLowerDamage)
        {
            Debug.Log("Far hit! " + dist);
            return currentGun.FarDamageMultiplier;
        }

        return 1;
    }

    private void CheckReload()
    {
        if (currentAmmo < currentGun.ClipCount && Input.GetButtonDown("Reload"))
        {
            ReloadGun();
        }
    }

    private void ReloadGun()
    {
        if (currentMaxAmmo <= 0 || reloading)
        {
            return;
        }

        timer = currentGun.ReloadSpeed;
        gunAnim.speed = 1 / currentGun.ReloadSpeed;
        gunAnim.SetTrigger("Reload");
        reloading = true;
    }
}
