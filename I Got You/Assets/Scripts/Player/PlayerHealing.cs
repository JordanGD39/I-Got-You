using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealing : MonoBehaviour
{
    [SerializeField] private int healingItems = 0;
    [SerializeField] private float healDelay = 2;
    [SerializeField] private int startingHealthGain = 60;
    [SerializeField] private int healthGain = 60;
    [SerializeField] private int healthGainPerSip = 1;
    [SerializeField] private float healthGainInterval = 0.25f;
    [SerializeField] private float afterHealthGainInterval = 0.1f;
    [SerializeField] private bool healing = false;
    private PlayerShoot playerShoot;
    private PlayerStats playerStats;
    private PlayerUI playerUI;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>();
        playerShoot = GetComponent<PlayerShoot>();
        playerUI = FindObjectOfType<PlayerUI>();

        healthGain = startingHealthGain;

        playerUI.HideChickenSoupBar();
        playerUI.UpdateHealthItemCount(healingItems);
    }

    // Update is called once per frame
    void Update()
    {
        CheckHealing();
    }

    public void AddHealthItem()
    {
        healingItems++;
        playerUI.UpdateHealthItemCount(healingItems);
    }

    private void CheckHealing()
    {
        if (!healing && !playerStats.PlayerAtMaxHealth() && healingItems > 0 && Input.GetButtonDown("Item"))
        {
            healing = true;
            anim.SetBool("Healing", true);
            playerShoot.PutWeaponAway();

            StartCoroutine(nameof(HealingPlayer));
        }

        if (healing && (Input.GetButtonUp("Item") || playerStats.PlayerAtMaxHealth()))
        {
            StopHealing();
            StopCoroutine(nameof(HealingPlayer));
        }
    }

    public IEnumerator HealingPlayer()
    {
        playerUI.UpdateChickenSoupBar(healthGain, startingHealthGain);

        yield return new WaitForSeconds(healDelay + healthGainInterval);

        while (healthGain > 0 && healing)
        {
            healthGain -= healthGainPerSip;
            playerUI.UpdateChickenSoupBar(healthGain, startingHealthGain);

            playerStats.Heal(healthGainPerSip);
            yield return new WaitForSeconds(healthGainInterval + afterHealthGainInterval);
        }

        if (healing)
        {
            StopHealing();
        }        
    }

    public void CheckEmpty()
    {
        if (healthGain <= 0)
        {
            healingItems--;
            healthGain = startingHealthGain;
            playerUI.UpdateHealthItemCount(healingItems);
        }        
    }

    public void StopHealing()
    {
        CheckEmpty();
        playerUI.HideChickenSoupBar();
        anim.SetBool("Healing", false);
        healing = false;
        playerShoot.PutWeaponBack();
    }
}
