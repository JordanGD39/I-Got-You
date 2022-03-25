using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    private int currentMaxHealth = 100;

    private PlayerUI playerUI;
    private int healthIncreaseCounter = 0;

    private void Start()
    {
        playerUI = FindObjectOfType<PlayerUI>();
        health = maxHealth;
        currentMaxHealth = maxHealth;
        playerUI.UpdateHealth(health);
        playerUI.UpdateMaxHealth(maxHealth);
    }

    public void Damage(int dmg)
    {
        health -= dmg;

        playerUI.UpdateHealth(health);

        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void IncreaseMaxHealth()
    {
        healthIncreaseCounter++;

        currentMaxHealth = Mathf.RoundToInt(maxHealth * (1 + (0.25f * (float)healthIncreaseCounter)));

        playerUI.UpdateMaxHealth(currentMaxHealth);
    }
}
