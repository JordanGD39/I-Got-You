using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerShoot : MonoBehaviourPun
{
    [SerializeField] private GunObject currentGun;
    public GunObject CurrentGun { get { return currentGun; } }
    [SerializeField] private GunObject secondaryGun;
    public GunObject SecondaryGun { get { return secondaryGun; } }
    [SerializeField] private GunHolder currentGunHolder;
    [SerializeField] private int currentAmmo = 0;
    public int CurrentAmmo { get { return currentAmmo; } }
    [SerializeField] private int currentMaxAmmo = 0;
    [SerializeField] private int secondaryGunAmmo = 0;
    public int CurrentSecondaryAmmo { get { return secondaryGunAmmo; } }
    [SerializeField] private int secondaryGunMaxAmmo = 0;
    [SerializeField] private bool canShoot = true;
    [SerializeField] private bool switching = false;
    private bool weaponGone = false;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private AudioClip emptyAmmo;
    [SerializeField] private float shootingDamageMultiplier = 1;

    private Dictionary<EnemyStats, DamageInfo> damageToEnemies = new Dictionary<EnemyStats, DamageInfo>();

    private Dictionary<string, GunHolder> weaponReference = new Dictionary<string, GunHolder>();

    private float timer = 0;
    private bool holdingTrigger = false;
    private bool prevHoldTrigger = false;
    private bool reloading = false;

    private PlayerUI playerUI;
    private PlayerStats playerStats;
    private EnemyManager enemyManager;    
    private WeaponsHolder weaponsHolder;    
    private AudioSource audioSource;    

    // Start is called before the first frame update
    void Start()
    {
        weaponsHolder = FindObjectOfType<WeaponsHolder>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        SavedPlayerStats savedStats = null;

        if (PlayersStatsHolder.instance.PlayerStatsSaved.Length > 0)
        {
            savedStats = PlayersStatsHolder.instance.PlayerStatsSaved[PhotonNetwork.IsConnected ? (photonView.OwnerActorNr - 1) : 0];

            if (savedStats != null && savedStats.guns[0] != null)
            {
                currentGun = savedStats.guns[0];
                secondaryGun = savedStats.guns[1];
                currentAmmo = savedStats.ammo[0];
                secondaryGunAmmo = savedStats.ammo[1];

                currentMaxAmmo = currentGun.MaxAmmoCount;

                if (secondaryGun != null)
                {
                    secondaryGunMaxAmmo = secondaryGun.MaxAmmoCount;
                }
            }
        }

        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }

        InputUI inputUI = FindObjectOfType<InputUI>();

        inputUI.OnTogglePausedGame += () => { canShoot = !canShoot; };

        enemyManager = FindObjectOfType<EnemyManager>();
        playerStats = GetComponent<PlayerStats>();
        playerUI = FindObjectOfType<PlayerUI>();

        if (savedStats == null || savedStats.guns[0] == null)
        {
            GiveFullAmmo(true);
        }
        else
        {
            playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
        }
        
        switching = true;
        StartChangingCurrentGun();
    }

    public void ModifyDamageStat(float multiplier)
    {
        shootingDamageMultiplier = multiplier;
    }

    public void GiveFullAmmo(bool secondary)
    {
        currentAmmo = currentGun.ClipCount;
        currentMaxAmmo = currentGun.MaxAmmoCount;

        if (secondaryGun != null && secondary)
        {
            secondaryGunAmmo = secondaryGun.ClipCount;
            secondaryGunMaxAmmo = secondaryGun.MaxAmmoCount;
        }

        audioSource.PlayOneShot(currentGun.SwitchSFX);
        playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
    }

    public void GiveAmmo(int ammoToGive)
    {
        currentAmmo = currentGun.ClipCount;
        currentMaxAmmo += ammoToGive;

        if (currentMaxAmmo > currentGun.MaxAmmoCount)
        {
            currentMaxAmmo = currentGun.MaxAmmoCount;
        }

        audioSource.PlayOneShot(currentGun.SwitchSFX);
        playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
    }

    public void GiveWeapon(GunObject gun)
    {
        if (secondaryGun == null)
        {
            secondaryGun = gun;

            secondaryGunAmmo = secondaryGun.ClipCount;
            secondaryGunMaxAmmo = secondaryGun.MaxAmmoCount;
        }
        else
        {
            currentGun = gun;
            PutWeaponAway();
            GiveFullAmmo(false);
        }

        StartChangingCurrentGun();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }

        if (canShoot && !switching && !weaponGone)
        {
            CheckShoot();
        }
        
        CheckInteract();

        if (playerStats.OnInteract == null && !weaponGone)
        {
            CheckReload();

            if (!switching)
            {
                CheckChangeSelectedWeapon();
            }            
        }
    }

    private void CheckShoot()
    {
        holdingTrigger = Input.GetAxisRaw("Attack") > 0.5f;

        bool attackPressed = Input.GetButtonDown("Attack") || (holdingTrigger && !prevHoldTrigger);

        if (currentAmmo < 0 || (currentMaxAmmo <= 0 && currentAmmo <= 0))
        {
            if (currentMaxAmmo <= 0 && attackPressed)
            {
                audioSource.PlayOneShot(emptyAmmo);
            }

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
            audioSource.PlayOneShot(currentGun.SwitchSFX);
        }
        
        bool attackHold = Input.GetButton("Attack") || holdingTrigger;

        prevHoldTrigger = holdingTrigger;

        if ((currentGun.ShootType == GunObject.ShootTypes.MANUAL || currentGun.ShootType == GunObject.ShootTypes.BURST) && attackPressed || currentGun.ShootType == GunObject.ShootTypes.AUTO && attackHold)
        {
            if (currentAmmo > 0)
            {
                timer = currentGun.ShootType != GunObject.ShootTypes.BURST ? currentGun.ShootDelay : currentGun.ShootDelay * currentGun.BulletCount;
            }
            else
            {
                ReloadGun();
                return;
            }

            damageToEnemies.Clear();

            if (currentGun.ShootType != GunObject.ShootTypes.BURST)
            {
                for (int i = 0; i < currentGun.BulletCount; i++)
                {
                    ShootCurrentGun(i);
                }

                currentAmmo -= 1;
                playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);
                audioSource.PlayOneShot(currentGun.FireSFX);

                DamageAllEnemiesInDictionary();
            }
            else
            {
                StartCoroutine(nameof(DelayBurstShooting));
            }

            currentGunHolder.GunAnim.SetTrigger("Shoot");
        }
    }

    private void DamageAllEnemiesInDictionary()
    {
        if (damageToEnemies.Count == 0)
        {
            return;
        }

        foreach (EnemyStats enemy in damageToEnemies.Keys)
        {
            DamageInfo damageInfo = damageToEnemies[enemy];

            if (playerStats.TankTauntScript != null && !playerStats.TankTauntScript.Taunting)
            {
                playerStats.TankTauntScript.AddCharge(damageInfo.weakspot ? 0.02f : 0.01f);
            }

            if (playerStats.SupportBurstHealScript != null)
            {
                playerStats.SupportBurstHealScript.AddCharge(damageInfo.weakspot ? 0.06f : 0.03f);
            }

            enemy.Damage(damageInfo.damage, damageInfo.direction);
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
            audioSource.PlayOneShot(currentGun.FireSFX);

            currentAmmo -= 1;
            playerUI.UpdateAmmo(currentAmmo, currentMaxAmmo);

            yield return new WaitForSeconds(currentGun.BurstDelay);
        }

        DamageAllEnemiesInDictionary();
    }

    private void ShootCurrentGun(int bulletCount)
    {
        if (PhotonNetwork.IsConnected && photonView.IsMine && bulletCount == 0)
        {
            photonView.RPC("ShootCurrentGunOthers", RpcTarget.Others);
        }

        currentGunHolder.MuzzleFlash.Play();

        RaycastHit hit;

        Vector3 dir = bulletCount == 0 ? shootPoint.forward : GetRandomShootingDirection();

        if (Physics.Raycast(shootPoint.position, dir, out hit, currentGun.MaxRange, hitLayer))
        {
            if (hit.collider.gameObject.CompareTag("EnemyCol"))
            {
                AddDamage(hit.collider.transform.root, dir, false);

                if (bulletCount == 0)
                {
                    playerUI.ShowHitMarker(hit.point, false);
                }                
            }
            else if (hit.collider.gameObject.CompareTag("HeadEnemyCol"))
            {
                AddDamage(hit.collider.transform.root, dir, true);

                if (bulletCount == 0)
                {
                    playerUI.ShowHitMarker(hit.point, true);
                }
            }
        }
    }

    private void AddDamage(Transform enemyRoot, Vector3 dir, bool headShot)
    {
        float damageMultiplier = 1;

        if (headShot)
        {
            damageMultiplier = currentGun.HeadShotMultiplier;
        }

        EnemyStats enemy = enemyManager.StatsOfAllEnemies[enemyRoot];
        if (damageToEnemies.ContainsKey(enemy))
        {
            damageToEnemies[enemy].damage += Mathf.RoundToInt(currentGun.Damage * damageMultiplier * shootingDamageMultiplier);
            damageToEnemies[enemy].direction = dir;
            return;
        }

        DamageInfo damageInfo = new DamageInfo();
        damageInfo.damage = Mathf.RoundToInt(currentGun.Damage * damageMultiplier * shootingDamageMultiplier);
        damageInfo.direction = dir;
        damageInfo.weakspot = headShot;

        damageToEnemies.Add(enemyManager.StatsOfAllEnemies[enemyRoot], damageInfo);
    }

    [PunRPC]
    void ShootCurrentGunOthers()
    {
        if (currentGun.ShootType != GunObject.ShootTypes.BURST)
        {
            currentGunHolder.GunAnim.SetTrigger("Shoot");
            currentGunHolder.MuzzleFlash.Play();
            audioSource.PlayOneShot(currentGun.FireSFX);
        }
        else
        {
            StartCoroutine(nameof(DelayBurstShootAnimOnly));
        }
    }

    private IEnumerator DelayBurstShootAnimOnly()
    {
        for (int i = 0; i < currentGun.BulletCount; i++)
        {
            currentGunHolder.GunAnim.SetTrigger("Shoot");
            currentGunHolder.MuzzleFlash.Play();
            audioSource.PlayOneShot(currentGun.FireSFX);

            yield return new WaitForSeconds(currentGun.BurstDelay);
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

        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("ReloadGunOthers", RpcTarget.Others);
        }

        timer = currentGun.ReloadSpeed;
        currentGunHolder.GunAnim.speed = 1 / currentGun.ReloadSpeed;
        currentGunHolder.GunAnim.SetTrigger("Reload");
        reloading = true;
    }

    [PunRPC]
    void ReloadGunOthers()
    {
        currentGunHolder.GunAnim.speed = 1 / currentGun.ReloadSpeed;
        currentGunHolder.GunAnim.SetTrigger("Reload");
    }

    private void CheckChangeSelectedWeapon()
    {
        if (Input.GetButtonDown("Switch") && secondaryGun != null)
        {
            reloading = false;
            timer = 0;
            currentGunHolder.GunAnim.speed = 1;
            currentGunHolder.GunAnim.ResetTrigger("Reload");
            PutWeaponAway();
            weaponGone = false;
            currentGunHolder.OnGunPutAway = ChangeWeaponAndAmmo;

            StartChangingCurrentGun();
        }
    }

    private void ChangeWeaponAndAmmo()
    {
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
    }

    private void StartChangingCurrentGun()
    {
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("UpdateCurrentVisibleGunOthers", RpcTarget.Others, (byte)weaponsHolder.SearchWeaponIndex(currentGun.name, currentGun.Primary), currentGun.Primary);
        }

        if (weaponReference.Count == 0)
        {
            foreach (GunHolder item in GetComponentsInChildren<GunHolder>(true))
            {
                weaponReference.Add(item.OwningGun.name, item);
                if (weaponReference.Count > 0)
                {
                    if (!photonView.IsMine)
                    {
                        item.gameObject.layer = 3;

                        foreach (Transform child in item.GetComponentsInChildren<Transform>(true))
                        {
                            child.gameObject.layer = 3;
                        }
                    }
                }

                if (weaponReference.Count > 1)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        if (currentGunHolder == null)
        {
            UpdateCurrentVisibleGun();
            return;
        }

        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            UpdateCurrentVisibleGun();
        }
        else
        {
            currentGunHolder.OnGunCanShoot = null;
            currentGunHolder.OnGunPutAway += UpdateCurrentVisibleGun;
        }        
    }

    private void UpdateCurrentVisibleGun()
    {
        if (currentGunHolder != null)
        {
            currentGunHolder.gameObject.SetActive(false);
        }
        
        currentGunHolder = weaponReference[currentGun.name];

        if (!PhotonNetwork.IsConnected || photonView.IsMine)
        {
            currentGunHolder.OnGunPutAway = null;
            currentGunHolder.OnGunCanShoot = () => { switching = false; };
        }        

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.PlayOneShot(currentGun.SwitchSFX);

        if (!weaponGone)
        {
            currentGunHolder.gameObject.SetActive(true);
        }

        if (currentGunHolder.GunAnim != null)
        {
            currentGunHolder.GunAnim.ResetTrigger("Reload");
        }
    }

    [PunRPC]
    void UpdateCurrentVisibleGunOthers(byte weaponIndex, bool primary)
    {
        if (weaponsHolder == null)
        {
            weaponsHolder = FindObjectOfType<WeaponsHolder>();
        }

        currentGun = primary ? weaponsHolder.PrimaryGuns[weaponIndex] : weaponsHolder.SecondaryGuns[weaponIndex];

        if (currentGunHolder != null)
        {
            currentGunHolder.gameObject.SetActive(false);
        }

        StartChangingCurrentGun();
    }

    private void CheckInteract()
    {
        if (Input.GetButtonDown("Interact") && playerStats.OnInteract != null)
        {
            playerStats.OnInteract(playerStats);
        }
    }

    public void PutWeaponAway()
    {
        if (switching)
        {
            currentGunHolder.gameObject.SetActive(false);
            return;
        }

        currentGunHolder.GunAnim.SetTrigger("PutAwayGun");
        switching = true;
        weaponGone = true;  
    }

    public void PutWeaponBack()
    {
        currentGunHolder.gameObject.SetActive(false);
        currentGunHolder.gameObject.SetActive(true);
        switching = false;
        weaponGone = false;        
    }
}

public class DamageInfo
{
    public int damage = 0;
    public Vector3 direction = Vector3.zero;
    public bool weakspot = false;
}
