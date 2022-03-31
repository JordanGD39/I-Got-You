using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class PlayerStats : MonoBehaviourPun
{
    public enum ClassNames { SCOUT, TANK, SUPPORT, BOMBER }
    [SerializeField] private ClassNames currentClass;
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    private int currentMaxHealth = 100;

    private PlayerUI playerUI;
    private int healthIncreaseCounter = 0;

    private void Start()
    {
        health = maxHealth;
        currentMaxHealth = maxHealth;

        if (photonView.IsMine)
        {
            playerUI = FindObjectOfType<PlayerUI>();
            playerUI.UpdateHealth(health);
            playerUI.UpdateMaxHealth(maxHealth);
        }      
    }

    public void Damage(int dmg)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        health -= dmg;
        playerUI.UpdateHealth(health);

        if (health <= 0)
        {
            Destroy(GetComponent<PlayerMovement>());
        }
    }

    public void IncreaseMaxHealth()
    {
        healthIncreaseCounter++;

        currentMaxHealth = Mathf.RoundToInt(maxHealth * (1 + (0.25f * (float)healthIncreaseCounter)));

        playerUI.UpdateMaxHealth(currentMaxHealth);
    }
}