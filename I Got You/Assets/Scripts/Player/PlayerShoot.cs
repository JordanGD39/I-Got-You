using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private GunObject currentGun;
    [SerializeField] private GunObject secondaryGun;
    [SerializeField] private GunHolder currentGunHolder;
    [SerializeField] private int currentAmmo = 0;
    [SerializeField] private int currentMaxAmmo = 0;
    [SerializeField] private int secondaryGunAmmo = 0;
    [SerializeField] private int secondaryGunMaxAmmo = 0;
    [SerializeField] private float distanceForExtraDamage = 20;
    [SerializeField] private float distanceForLowerDamage = 100;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LayerMask hitLayer;

    private Dictionary<string, GunHolder> weaponReference = new Dictionary<string, GunHolder>();

    private float timer = 0;
    private bool holdingTrigger = false;
    private bool prevHoldTrigger = false;
    private bool reloading = false;

    private PlayerUI playerUI;
    private EnemyManager enemyManager;

    // Start is called before the first frame update
    void Start()
    {
        enemyManager = FindObjectOfType<EnemyManager>();

        SetAmmo();

        playerUI = FindObjectOfType<PlayerUI>();
        playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);

        foreach (GunHolder item in GetComponentsInChildren<GunHolder>())
        {
            weaponReference.Add(item.OwningGun.name, item);
            item.gameObject.SetActive(false);
        }

        UpdateCurrentVisibleGun();
    }

    private void SetAmmo()
    {
        currentAmmo = currentGun.ClipCount;
        currentMaxAmmo = currentGun.MaxAmmoCount;

        if (secondaryGun != null)
        {
            secondaryGunAmmo = secondaryGun.ClipCount;
            secondaryGunMaxAmmo = secondaryGun.MaxAmmoCount;
        }        
    }

    // Update is called once per frame
    void Update()
    {
        CheckShoot();
        CheckReload();
        CheckChangeSelectedWeapon();
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
            currentGunHolder.GunAnim.speed = 1;
        }

        holdingTrigger = Input.GetAxisRaw("Attack") > 0.5f;

        bool attackPressed = Input.GetButtonDown("Attack") || (holdingTrigger && !prevHoldTrigger);
        bool attackHold = Input.GetButton("Attack") || holdingTrigger;

        prevHoldTrigger = holdingTrigger;

        if ((currentGun.ShootType == GunObject.ShootTypes.MANUAL || currentGun.ShootType == GunObject.ShootTypes.BURST) && attackPressed || currentGun.ShootType == GunObject.ShootTypes.AUTO && attackHold)
        {
            if (currentGun.ShootType != GunObject.ShootTypes.BURST)
            {
                for (int i = 0; i < currentGun.BulletCount; i++)
                {
                    ShootCurrentGun(i);
                }

                currentAmmo -= 1;
                playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
            }
            else
            {
                StartCoroutine(nameof(DelayBurstShooting));
            }

            currentGunHolder.GunAnim.SetTrigger("Shoot");

            if (currentAmmo > 0)
            {
                timer = currentGun.ShootType != GunObject.ShootTypes.BURST ? currentGun.ShootDelay : currentGun.ShootDelay * currentGun.BulletCount;
            }
            else
            {
                ReloadGun();
            }
        }
    }

    private IEnumerator DelayBurstShooting()
    {
        for (int i = 0; i < currentGun.BulletCount; i++)
        {
            if (currentAmmo <= 0)
            {
                break;
            }

            ShootCurrentGun(0);

            currentAmmo -= 1;
            playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);

            yield return new WaitForSeconds(currentGun.BurstDelay);
        }
    }

    private void ShootCurrentGun(int bulletCount)
    {
        currentGunHolder.MuzzleFlash.Play();

        RaycastHit hit;

        if (Physics.Raycast(shootPoint.position, bulletCount == 0 ? shootPoint.forward : GetRandomShootingDirection(), out hit, currentGun.MaxRange, hitLayer))
        {
            if (hit.collider.gameObject.CompareTag("EnemyCol"))
            {
                enemyManager.StatsOfAllEnemies[hit.collider.transform.parent].Damage(Mathf.RoundToInt(currentGun.Damage));

                if (bulletCount == 0)
                {
                    playerUI.ShowHitMarker(hit.point);
                }                
            }
            else if (hit.collider.gameObject.CompareTag("HeadEnemyCol"))
            {
                enemyManager.StatsOfAllEnemies[hit.collider.transform.parent].Damage(Mathf.RoundToInt((float)currentGun.Damage * currentGun.HeadShotMultiplier));

                if (bulletCount == 0)
                {
                    playerUI.ShowHitMarker(hit.point);
                }
            }
        }
    }

    private void CheckReload()
    {
        if (currentAmmo < currentGun.ClipCount && Input.GetButtonDown("Reload"))
        {
            ReloadGun();
        }
    }

    private Vector3 GetRandomShootingDirection()
    {
        Vector3 randomLocalPos = new Vector3(Random.Range(-currentGun.SpreadAngle, currentGun.SpreadAngle), Random.Range(-currentGun.SpreadAngle, currentGun.SpreadAngle), 0);

        randomLocalPos = transform.TransformVector(randomLocalPos);

        return shootPoint.forward + randomLocalPos;
    }

    private void ReloadGun()
    {
        if (currentMaxAmmo <= 0 || reloading)
        {
            return;
        }

        timer = currentGun.ReloadSpeed;
        currentGunHolder.GunAnim.speed = 1 / currentGun.ReloadSpeed;
        currentGunHolder.GunAnim.SetTrigger("Reload");
        reloading = true;
    }

    private void CheckChangeSelectedWeapon()
    {
        if (Input.GetButtonDown("Switch") && secondaryGun != null)
        {
            reloading = false;
            timer = 0;
            currentGunHolder.GunAnim.speed = 1;
            currentGunHolder.GunAnim.ResetTrigger("Reload");
            currentGunHolder.gameObject.SetActive(false);
            GunObject secondGun = secondaryGun;
            int secondaryAmmo = secondaryGunAmmo;
            int secondaryMaxAmmo = secondaryGunMaxAmmo;
   
            secondaryGun = currentGun;
            currentGun = secondGun;

            secondaryGunAmmo = currentAmmo;
            currentAmmo = secondaryAmmo;

            secondaryGunMaxAmmo = currentMaxAmmo;
            currentMaxAmmo = secondaryMaxAmmo;

            playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
            UpdateCurrentVisibleGun();
        }
    }

    private void UpdateCurrentVisibleGun()
    {
        currentGunHolder = weaponReference[currentGun.name];

        currentGunHolder.gameObject.SetActive(true);

        if (currentGunHolder.GunAnim != null)
        {
            currentGunHolder.GunAnim.ResetTrigger("Reload");
        }
    }
}
