using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerStats : MonoBehaviourPun
{
    public enum ClassNames { SCOUT, TANK, SUPPORT, BOMBER }
    [SerializeField] private ClassNames currentClass;
    public ClassNames CurrentClass { get { return currentClass; } }
    [SerializeField] private int health = 100;
    public int Health { get { return health; } }
    [SerializeField] private int maxHealth = 100;
    private int currentMaxHealth = 100;
    [SerializeField] private float shieldHealth = 50;
    [SerializeField] private int startingShieldHealth = 50;
    [SerializeField] private float regenDelay = 1;
    [SerializeField] private float regenSpeed = 0.25f;
    [SerializeField] private float invincibilityTime = 1;
    [SerializeField] private Transform classModels;
    public Transform ClassModels { get { return classModels; } }
    [SerializeField] private TextMeshProUGUI usernameText;
    public bool HasShieldHealth { get; set; } = false;

    private PlayerUI playerUI;
    private PlayerRevive playerRevive;
    public PlayerShoot PlayerShootScript { get; private set; }
    public PlayerHealing PlayerHealingScript { get; private set; }
    private Animator anim;
    private int healthIncreaseCounter = 0;
    private bool isDown = false;
    public bool IsDown { get { return isDown; } }
    public bool IsDead { get; set; } = false;
    private bool invincible = false;

    public delegate void Interact(PlayerStats playerStats);
    public Interact OnInteract;
    public delegate void InteractHoldStop(PlayerStats playerStats);
    public InteractHoldStop OnInteractHoldStop;
    [SerializeField] private List<InteractableObject> inventoryOfInteractables = new List<InteractableObject>();
    public List<InteractableObject> InventoryOfInteractables { get { return inventoryOfInteractables; } }

    public TankTaunt TankTauntScript { get; set; } = null;
    public SupportBurstHeal SupportBurstHealScript { get; set; } = null;

    private void Awake()
    {
        if (PlayerChoiceManager.instance != null)
        {
            foreach (int item in PlayerChoiceManager.instance.ChosenClasses.Keys)
            {
                Debug.Log("Actor: " + (photonView.OwnerActorNr - 1) +  " KeyNmr: " + item);
            }
            int playerIndex = PhotonNetwork.IsConnected ? photonView.OwnerActorNr - 1 : 0;
            currentClass = PlayerChoiceManager.instance.ChosenClasses[playerIndex];
        }
    }

    private void Start()
    {
        shieldHealth = startingShieldHealth;

        SavedPlayerStats savedStats = null;

        if (PlayersStatsHolder.instance.PlayerStatsSaved.Length > 0)
        {
            savedStats = PlayersStatsHolder.instance.PlayerStatsSaved[PhotonNetwork.IsConnected ? (photonView.OwnerActorNr - 1) : 0];
        }        

        if (savedStats == null)
        {
            health = maxHealth;
        }
        else
        {
            health = savedStats.health;
        }

        currentMaxHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
        PlayerShootScript = GetComponent<PlayerShoot>();
        PlayerHealingScript = GetComponent<PlayerHealing>();

        classModels.GetChild((int)currentClass).gameObject.SetActive(true);

        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            usernameText.transform.parent.gameObject.SetActive(false);
            SetLayerRecursively(classModels.GetChild((int)currentClass).gameObject, 10);

            playerUI = FindObjectOfType<PlayerUI>();

            if (currentClass != ClassNames.TANK)
            {
                playerUI.HideShieldHealth();
            }

            playerUI.UpdateHealth(health, maxHealth);
            //playerUI.UpdateMaxHealth(maxHealth);
            playerUI.UpdateShieldHealth(shieldHealth, startingShieldHealth);
            playerRevive = GetComponent<PlayerRevive>();
        }    
        else
        {
            usernameText.text = photonView.Owner.NickName;
        }
    }

    private void Update()
    {
        if (photonView.IsMine && Input.GetButtonUp("Interact"))
        {
            OnInteractHoldStop?.Invoke(this);
        }
    }

    public void PickUpInteractable(InteractableObject interactable)
    {
        inventoryOfInteractables.Add(interactable);
    }

    public void ModifyHealthStat(float multiplier)
    {
        if (health >= maxHealth)
        {
            health = Mathf.RoundToInt((float)health * multiplier);
        }
        
        maxHealth = Mathf.RoundToInt((float)maxHealth * multiplier);
    }

    public void RemoveInteractable(InteractableObject interactable)
    {
        inventoryOfInteractables.Remove(interactable);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null || child.GetComponent<SpriteRenderer>() != null)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void Heal(int healthGain)
    {
        health += healthGain;

        if (health >= currentMaxHealth)
        {
            health = currentMaxHealth;
        }

        playerUI.UpdateHealth(health, maxHealth);
    }

    public bool PlayerAtMaxHealth(float multiplier)
    {
        return health >= (float)currentMaxHealth * multiplier;
    }

    public void Damage(int dmg)
    {
        if (!photonView.IsMine || (invincible && dmg < 500))
        {
            return;
        }

        playerUI.ShowBloodScreen();

        if (TankTauntScript != null)
        {
            if (TankTauntScript.Taunting)
            {
                dmg = Mathf.RoundToInt((float)dmg * TankTauntScript.DamageMultiplier);
            }
            else
            {
                TankTauntScript.AddCharge(0.01f);
            }
        }

        if (SupportBurstHealScript != null)
        {
            SupportBurstHealScript.AddCharge(0.02f);
        }

        if (isDown)
        {
            playerRevive.ResetDamageTimer();
            return;
        }
        
        if (HasShieldHealth && shieldHealth > 0)
        {
            shieldHealth -= dmg;

            bool moreDamageThanShield = false;

            if (shieldHealth < 0)
            {
                moreDamageThanShield = true;

                dmg += Mathf.RoundToInt(shieldHealth);

                shieldHealth = 0;
            }

            playerUI.UpdateShieldHealth(shieldHealth, startingShieldHealth);

            StopCoroutine(nameof(StartShieldRegeneration));
            StartCoroutine(nameof(StartShieldRegeneration));

            if (!moreDamageThanShield)
            {
                return;
            }            
        }

        if (dmg > 500)
        {
            health = 0;
        }
        else
        {
            health -= dmg;
        }

        if (health <= 0)
        {
            health = 0;
            isDown = true;
            anim.SetBool("Down", true);

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("ShowDownOthers", RpcTarget.Others);
            }

            playerRevive.StartTimer();
            StopCoroutine(nameof(StartShieldRegeneration));
            playerUI.UpdateShieldHealth(0, startingShieldHealth);
            OnInteractHoldStop?.Invoke(this);
        }

        playerUI.UpdateHealth(health, maxHealth);
    }

    [PunRPC]
    void ShowDownOthers()
    {
        anim.SetBool("Down", true);
    }

    public void Revived()
    {
        health = maxHealth;
        shieldHealth = startingShieldHealth;
        isDown = false;
        anim.SetBool("Down", false);

        if (photonView.IsMine)
        {
            invincible = true;
            Invoke(nameof(RemoveInvincibility), invincibilityTime);

            if (playerUI != null)
            {
                playerUI.UpdateHealth(health, maxHealth);
                playerUI.UpdateShieldHealth(shieldHealth, startingShieldHealth);
            }
        }    
    }

    private void RemoveInvincibility()
    {
        invincible = false;
    }

    private IEnumerator StartShieldRegeneration()
    {
        yield return new WaitForSeconds(regenDelay);

        while (shieldHealth < startingShieldHealth)
        {
            shieldHealth += regenSpeed * Time.deltaTime;

            if (shieldHealth > startingShieldHealth)
            {
                shieldHealth = startingShieldHealth;
            }

            playerUI.UpdateShieldHealth(shieldHealth, startingShieldHealth);

            yield return null;
        }
    }

    public void IncreaseMaxHealth()
    {
        healthIncreaseCounter++;

        currentMaxHealth = Mathf.RoundToInt(maxHealth * (1 + (0.25f * (float)healthIncreaseCounter)));

        //playerUI.UpdateMaxHealth(currentMaxHealth);
    }

    public void SavePlayerStats()
    {
        SavedPlayerStats savedStats = new SavedPlayerStats();
        savedStats.health = health;
        savedStats.guns[0] = PlayerShootScript.CurrentGun;
        savedStats.guns[1] = PlayerShootScript.SecondaryGun;
        savedStats.ammo[0] = PlayerShootScript.CurrentAmmo;
        savedStats.ammo[1] = PlayerShootScript.CurrentSecondaryAmmo;

        if (TankTauntScript != null)
        {
            savedStats.abilityCharge = TankTauntScript.Charge;
        }

        if (SupportBurstHealScript != null)
        {
            savedStats.abilityCharge = SupportBurstHealScript.Charge;
        }

        PlayersStatsHolder playersStatsHolder = PlayersStatsHolder.instance;

        playersStatsHolder.PlayerStatsSaved[PhotonNetwork.IsConnected ? (photonView.OwnerActorNr - 1) : 0] = savedStats;
    }
}