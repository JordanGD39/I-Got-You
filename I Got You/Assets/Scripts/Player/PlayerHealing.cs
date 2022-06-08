using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;

public class PlayerHealing : MonoBehaviourPun
{
    [SerializeField] private int healingItems = 0;
    public int HealingItems { get { return healingItems; } }
    [SerializeField] private int maxHealthItems = 2;
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
    [SerializeField] private VisualEffect drinkEffect;

    private bool buffed = false;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }
        anim = GetComponentInChildren<Animator>();
        playerStats = GetComponent<PlayerStats>();
        playerShoot = GetComponent<PlayerShoot>();

        if (playerUI == null)
        {
            playerUI = FindObjectOfType<PlayerUI>();
        }

        if (!buffed)
        {
            healthGain = startingHealthGain;
        }

        playerUI.HideChickenSoupBar();
        playerUI.UpdateHealthItemCount(healingItems);
        playerUI.UpdateMaxHealthItemCount(maxHealthItems);
    }

    public void ModifyHealingStat(float multiplier)
    {
        if (!buffed)
        {
            healthGain = startingHealthGain;
        }

        buffed = true;
        healthGain = Mathf.RoundToInt(healthGain * multiplier);
        maxHealthItems = Mathf.RoundToInt(maxHealthItems * multiplier);

        if (playerUI == null)
        {
            playerUI = FindObjectOfType<PlayerUI>();
        }

        playerUI.UpdateMaxHealthItemCount(maxHealthItems);
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }
        CheckHealing();
    }

    public bool AddHealthItem()
    {
        if (healingItems >= maxHealthItems)
        {
            return false;
        }

        healingItems++;
        playerUI.UpdateHealthItemCount(healingItems);

        return true;
    }

    private void CheckHealing()
    {
        if (!healing && !playerStats.PlayerAtMaxHealth(1) && healingItems > 0 && Input.GetButtonDown("Item"))
        {
            healing = true;
            anim.SetBool("Healing", true);
            playerShoot.PutWeaponAway();

            StartCoroutine(nameof(HealingPlayer));
        }

        if (healing && (Input.GetButtonUp("Item") || playerStats.PlayerAtMaxHealth(1)))
        {
            StopHealing();
            StopCoroutine(nameof(HealingPlayer));
        }
    }

    public IEnumerator HealingPlayer()
    {
        playerUI.UpdateChickenSoupBar(healthGain, startingHealthGain);

        yield return new WaitForSeconds(healDelay + healthGainInterval);
        drinkEffect.gameObject.SetActive(true);
        drinkEffect.Play();

        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("ShowEffectOthers", RpcTarget.Others);
        }

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

    [PunRPC]
    void ShowEffectOthers()
    {
        drinkEffect.gameObject.SetActive(true);
        drinkEffect.Play();
    }

    [PunRPC]
    void StopEffectOthers()
    {
        drinkEffect.Stop();
    }

    public void StopHealing()
    {
        CheckEmpty();
        playerUI.HideChickenSoupBar();
        anim.SetBool("Healing", false);
        healing = false;
        playerShoot.PutWeaponBack();
        drinkEffect.Stop();

        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("StopEffectOthers", RpcTarget.Others);
        }
    }

    public void RemoveSoup(int amount)
    {
        healthGain -= amount;

        if (healthGain <= 0)
        {
            healthGain = 0;
            healingItems--;
            playerUI.UpdateHealthItemCount(healingItems);
        }
    }

    public void PlayDrinkEffect()
    {
        drinkEffect.gameObject.SetActive(true);
        drinkEffect.Play();

        Invoke(nameof(StopDrinkEffect), 0.5f);
    }

    public void StopDrinkEffect()
    {
        drinkEffect.Stop();
    }
}