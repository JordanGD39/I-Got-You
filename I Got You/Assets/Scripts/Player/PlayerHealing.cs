using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealing : MonoBehaviour
{
    [SerializeField] private int healingItems = 0;
    [SerializeField] private float healDelay = 2;
    [SerializeField] private int startingHealthGain = 60;
    [SerializeField] private int healthGain = 60;
    [SerializeField] private float healthGainInterval = 0.25f;
    [SerializeField] private bool healing = false;
    private PlayerShoot playerShoot;
    private PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerShoot = GetComponent<PlayerShoot>();

        healthGain = startingHealthGain;
    }

    // Update is called once per frame
    void Update()
    {
        CheckHealing();
    }

    private void CheckHealing()
    {
        if (!healing && !playerStats.PlayerAtMaxHealth() && healingItems > 0 && Input.GetButtonDown("Item"))
        {
            healthGain = startingHealthGain;
            healing = true;

            StartCoroutine(nameof(HealingPlayer));
        }

        if (healing && (Input.GetButtonUp("Item") || playerStats.PlayerAtMaxHealth()))
        {
            StopHealing();
            StopCoroutine(nameof(HealingPlayer));
        }
    }

    private IEnumerator HealingPlayer()
    {
        yield return new WaitForSeconds(healDelay);

        while (healthGain > 0)
        {
            healthGain--;

            playerStats.Heal(1);
            yield return new WaitForSeconds(healthGainInterval);
        }

        healingItems--;
        StopHealing();
    }

    private void StopHealing()
    {
        healing = false;
    }
}
