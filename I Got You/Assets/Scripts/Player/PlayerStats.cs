using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerStats : MonoBehaviourPun
{
    public enum ClassNames { SCOUT, TANK, SUPPORT, BOMBER }
    [SerializeField] private ClassNames currentClass;
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    private int currentMaxHealth = 100;

    private PlayerUI playerUI;
    private PlayerRevive playerRevive;
    public PlayerShoot PlayerShootScript { get; private set; }
    private Animator anim;
    private int healthIncreaseCounter = 0;
    private bool isDown = false;
    public bool IsDown { get { return isDown; } }
    public bool IsDead { get; set; } = false;

    public delegate void Interact(PlayerStats playerStats);
    public Interact OnInteract;

    private void Start()
    {
        health = maxHealth;
        currentMaxHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
        PlayerShootScript = GetComponent<PlayerShoot>();

        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            playerUI = FindObjectOfType<PlayerUI>();
            playerUI.UpdateHealth(health);
            playerUI.UpdateMaxHealth(maxHealth);
            playerRevive = GetComponent<PlayerRevive>();
        }      
    }

    public void Heal(int healthGain)
    {
        if (health >= currentMaxHealth)
        {
            health = currentMaxHealth;
            return;
        }

        health += healthGain;
        playerUI.UpdateHealth(health);
    }

    public bool PlayerAtMaxHealth()
    {
        return health >= currentMaxHealth;
    }

    public void Damage(int dmg)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        playerUI.ShowBloodScreen();

        if (isDown)
        {
            playerRevive.ResetDamageTimer();
            return;
        }

        health -= dmg;

        if (health <= 0)
        {
            health = 0;
            isDown = true;
            anim.SetBool("Down", true);
            playerRevive.StartTimer();
        }

        playerUI.UpdateHealth(health);
    }

    public void IncreaseMaxHealth()
    {
        healthIncreaseCounter++;

        currentMaxHealth = Mathf.RoundToInt(maxHealth * (1 + (0.25f * (float)healthIncreaseCounter)));

        playerUI.UpdateMaxHealth(currentMaxHealth);
    }
}