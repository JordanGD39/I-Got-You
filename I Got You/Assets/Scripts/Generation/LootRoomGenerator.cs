using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LootRoomGenerator : MonoBehaviourPun
{
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

    [SerializeField] private List<LootObject> allLoot = new List<LootObject>();
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

        doorOpen.OnOpenedDoor_OpenedDoor += PlaceLoot;
        doorOutHere.OnOpenedDoor_OpenedDoor += TurnOffLootOthers;
        doorOpen.gameObject.SetActive(false);        
    }

    private void UpdateLootBasedOnPlayers()
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
        Debug.Log("CALLED");

        foreach (LootObject loot in allLoot)
        {
            float rand = Random.Range(0, 100);

            //No loot
            if (rand > dropChance)
            {
                Debug.Log("NADA");
                loot.gameObject.SetActive(false);
                continue;
            }

            rand = Random.Range(0, 100);

            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("ShowLootBoxForOthers", RpcTarget.Others, (byte)allLoot.IndexOf(loot));
            }

            Debug.Log("YES");
            loot.gameObject.SetActive(true);

            float chance = weaponDropChance + (weaponDropChanceModifier * difficultyManager.DifficultyLevel);

            if (chance > weaponDropChanceLimit)
            {
                chance = weaponDropChanceLimit;
            }

            //Check to place ammo or weapon
            if (rand < chance)
            {
                //Place weapon
                rand = Random.Range(0, 100);

                chance = smallWeaponDropChance - (smallWeaponDropChanceModifier * difficultyManager.DifficultyLevel);

                if (rand < chance)
                {
                    //Place small weapon
                    loot.UpdateLootType(LootObject.LootTypes.SECONDARYWEAPON, -1);
                }
                else
                {
                    //Place large weapon
                    loot.UpdateLootType(LootObject.LootTypes.PRIMARYWEAPON, -1);
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
                    loot.UpdateLootType(LootObject.LootTypes.SMALLAMMO, -1);
                }
                else
                {
                    //Place medium or large ammo
                    rand = Random.Range(0, 100);

                    chance = largeAmmoDropChance + (largeAmmoDropChanceModifier * difficultyManager.DifficultyLevel);

                    if (rand > chance)
                    {
                        //Place medium ammo
                        loot.UpdateLootType(LootObject.LootTypes.MEDIUMAMMO, -1);
                    }
                    else
                    {
                        //Place large ammo
                        loot.UpdateLootType(LootObject.LootTypes.LARGEAMMO, -1);
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
