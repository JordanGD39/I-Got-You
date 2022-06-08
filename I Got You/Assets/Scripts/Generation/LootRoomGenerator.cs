using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LootRoomGenerator : MonoBehaviourPun
{
    public enum Rarities { COMMON, UNCOMMON, RARE, EPIC};

    [SerializeField] private float healthDropChance = 20;
    [SerializeField] private float healthDropMin = 5;
    [SerializeField] private float healthDropChanceModifier = 2;
    [SerializeField] private float healthDropChanceIncreasePlayerHealth = 20;
    [SerializeField] private float minPlayerHealthForMaxChance = 20;

    [SerializeField] private float weaponDropChance = 10;
    [SerializeField] private float weaponDropChanceModifier = 3;
    [SerializeField] private float weaponDropChanceLimit = 40;
    [SerializeField] private float smallWeaponDropChance = 70;
    [SerializeField] private float smallWeaponDropChanceModifier = 2;

    [SerializeField] private float mediumAmmoDropChance = 25;
    [SerializeField] private float mediumAmmoDropChanceModifier = 2;
    [SerializeField] private float largeAmmoDropChance = 10;
    [SerializeField] private float largeAmmoDropChanceModifier = 2;

    [SerializeField] private float startingDropChance = 40;
    [SerializeField] private float dropChance = 40;

    [SerializeField] private Vector2[] commonRarityChance;
    [SerializeField] private Vector2[] uncommonRarityChance;
    [SerializeField] private Vector2[] rareRarityChance;
    [SerializeField] private Vector2[] epicRarityChance;
    [SerializeField] private int[] floorRarityChanges = { 0, 5, 15, 25};
    [SerializeField] private float rarityModifier = 2;

    [SerializeField] private List<LootObject> allLoot = new List<LootObject>();
    public List<LootObject> AllLoot { get { return allLoot; } }
    [SerializeField] private DoorOpen doorOpen;
    [SerializeField] private DoorOpen doorOutHere;
    [SerializeField] private List<LootObject> onePlayerLootDrops = new List<LootObject>();
    [SerializeField] private List<LootObject> twoPlayerLootDrops = new List<LootObject>();
    [SerializeField] private List<LootObject> threePlayerLootDrops = new List<LootObject>();
    private DifficultyManager difficultyManager;
    private PlayerManager playerManager;

    private int playerCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        difficultyManager = FindObjectOfType<DifficultyManager>();
        playerManager = FindObjectOfType<PlayerManager>();

        if (doorOpen != null)
        {
            doorOpen.OnOpenedDoor += PlaceLoot;
        }

        if (doorOutHere != null)
        {
            doorOutHere.OnOpenedDoor += TurnOffLootOthers;
        }

        foreach (LootObject item in allLoot)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void UpdateLootBasedOnPlayers()
    {
        playerCount = playerManager.Players.Count;

        dropChance = startingDropChance;
        dropChance += (4 - playerCount) * 10;

        switch (playerCount)
        {
            case 1:
                allLoot = onePlayerLootDrops;
                break;
            case 2:
                allLoot = twoPlayerLootDrops;
                break;
            case 3:
                allLoot = threePlayerLootDrops;
                break;
        }
    }

    public void PlaceLoot()
    {
        UpdateLootBasedOnPlayers();
        //Debug.Log("CALLED");

        foreach (LootObject loot in allLoot)
        {
            float rand = Random.Range(0, 100);

            //No loot
            if (rand > dropChance)
            {
                //Debug.Log("NADA");
                loot.gameObject.SetActive(false);
                continue;
            }

            //Debug.Log("YES");
            loot.gameObject.SetActive(true);

            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("ShowLootBoxForOthers", RpcTarget.Others, (byte)allLoot.IndexOf(loot));
            }

            rand = Random.Range(0, 100);

            float itemChance = healthDropChance - (healthDropChanceModifier * difficultyManager.DifficultyLevel);

            if (itemChance < healthDropMin)
            {
                itemChance = healthDropMin;
            }

            float currentAllPlayersHealth = 0;
            int maxHealthers = 0;

            for (int i = 0; i < playerManager.Players.Count; i++)
            {
                currentAllPlayersHealth += playerManager.Players[i].Health;

                if (playerManager.Players[i].PlayerAtMaxHealth())
                {
                    maxHealthers++;
                }
            }

            if (maxHealthers < playerManager.Players.Count && currentAllPlayersHealth > 0)
            {
                float minAllPlayersHealth = minPlayerHealthForMaxChance * playerManager.Players.Count;
                itemChance += (float)healthDropChanceIncreasePlayerHealth * (minAllPlayersHealth / currentAllPlayersHealth);
            }
            //Debug.Log("Chance: " + itemChance + " num: " + rand);

            if (rand <= itemChance)
            {
                loot.UpdateLootType(LootObject.LootTypes.HEALTH, -1, 0);
                continue;
            }

            rand = Random.Range(0, 100);

            float chance = weaponDropChance + (weaponDropChanceModifier * difficultyManager.DifficultyLevel);

            if (chance > weaponDropChanceLimit)
            {
                chance = weaponDropChanceLimit;
            }

            //Check to place ammo or weapon
            if (rand < chance)
            {
                //Choose rarity index
                int rarityChanceIndex = 0;

                for (int i = 0; i < floorRarityChanges.Length; i++)
                {
                    rarityChanceIndex = i;

                    if (GameManager.instance.Floor > floorRarityChanges[i])
                    {
                        break;
                    }
                }

                //Rarity chance
                rand = Random.Range(0, 100);

                Rarities rarity = Rarities.COMMON;

                if (rand >= uncommonRarityChance[rarityChanceIndex].x && rand <= uncommonRarityChance[rarityChanceIndex].y)
                {
                    rarity = Rarities.UNCOMMON;
                }
                else if (rand >= rareRarityChance[rarityChanceIndex].x && rand <= rareRarityChance[rarityChanceIndex].y)
                {
                    rarity = Rarities.RARE;
                }
                else if (rand >= epicRarityChance[rarityChanceIndex].x && rand <= epicRarityChance[rarityChanceIndex].y)
                {
                    rarity = Rarities.EPIC;
                }

                //Place weapon
                rand = Random.Range(0, 100);

                chance = smallWeaponDropChance - (smallWeaponDropChanceModifier * difficultyManager.DifficultyLevel);

                if (rand < chance)
                {
                    //Place small weapon
                    loot.UpdateLootType(LootObject.LootTypes.SECONDARYWEAPON, -1, rarity);
                }
                else
                {
                    //Place large weapon
                    loot.UpdateLootType(LootObject.LootTypes.PRIMARYWEAPON, -1, rarity);
                }
            }
            else
            {
                //Place ammo
                rand = Random.Range(0, 100);

                chance = mediumAmmoDropChance + (mediumAmmoDropChanceModifier * difficultyManager.DifficultyLevel);

                if (rand > chance)
                {
                    //Place small Ammo
                    loot.UpdateLootType(LootObject.LootTypes.SMALLAMMO, -1, 0);
                }
                else
                {
                    //Place medium or large ammo
                    rand = Random.Range(0, 100);

                    chance = largeAmmoDropChance + (largeAmmoDropChanceModifier * difficultyManager.DifficultyLevel);

                    if (rand > chance)
                    {
                        //Place medium ammo
                        loot.UpdateLootType(LootObject.LootTypes.MEDIUMAMMO, -1, 0);
                    }
                    else
                    {
                        //Place large ammo
                        loot.UpdateLootType(LootObject.LootTypes.LARGEAMMO, -1, 0);
                    }
                }
            }
        }
    }

    public void TurnOffLootOthers()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            foreach (LootObject loot in allLoot)
            {
                loot.gameObject.SetActive(false);
            }
        }
    }

    [PunRPC]
    void ShowLootBoxForOthers(byte index)
    {
        UpdateLootBasedOnPlayers();
        Debug.Log("YES");

        allLoot[index].gameObject.SetActive(true);
    }
}
