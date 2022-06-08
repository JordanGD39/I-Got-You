using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LootObject : InteractableObject
{
    public enum LootTypes { PRIMARYWEAPON, SECONDARYWEAPON, SMALLAMMO, MEDIUMAMMO, LARGEAMMO, HEALTH }
    [SerializeField] private LootTypes lootType = LootTypes.SMALLAMMO;

    [SerializeField] private GameObject ammoCrates;
    [SerializeField] private GameObject weapons;
    [SerializeField] private GameObject items;
    [SerializeField] private int smallAmmoDrop = 50;
    [SerializeField] private int mediumAmmoDrop = 100;
    private WeaponsHolder weaponsHolder;

    public GunObject currentGun { get; private set; }
    private int ammo = -1;

    private LootRoomGenerator.Rarities currentRarity = LootRoomGenerator.Rarities.COMMON;

    public void UpdateLootType(LootTypes type, int weaponIndex, LootRoomGenerator.Rarities rarity)
    {
        currentRarity = rarity;

        if (weaponsHolder == null)
        {
            weaponsHolder = FindObjectOfType<WeaponsHolder>();
            playerManager = FindObjectOfType<PlayerManager>();
        }

        lootType = type;

        if (type == LootTypes.PRIMARYWEAPON || type == LootTypes.SECONDARYWEAPON)
        {
            weapons.SetActive(true);
            items.SetActive(false);
            ammoCrates.SetActive(false);

            DeactivateAllWeapons();
        }
        else if(type == LootTypes.HEALTH)
        {
            items.SetActive(true);
            weapons.SetActive(false);
            ammoCrates.SetActive(false);

            items.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            weapons.SetActive(false);
            items.SetActive(false);
            ammoCrates.SetActive(true);
            ammoCrates.transform.GetChild(0).gameObject.SetActive(false);
            ammoCrates.transform.GetChild(1).gameObject.SetActive(false);
            ammoCrates.transform.GetChild(2).gameObject.SetActive(false);
        }

        int rand = 0;

        switch (lootType)
        {
            case LootTypes.PRIMARYWEAPON:
                weapons.transform.GetChild(0).gameObject.SetActive(true);
                weapons.transform.GetChild(1).gameObject.SetActive(false);

                rand = weaponIndex < 0 ? Random.Range(0, weaponsHolder.PrimaryGuns.Count) : weaponIndex;

                currentGun = weaponsHolder.PrimaryGuns[rand];

                weaponIndex = rand;

                Transform weaponParent = weapons.transform.GetChild(0).GetChild(rand);

                weaponParent.gameObject.SetActive(true);

                if (currentGun.HasRarity)
                {
                    for (int i = 0; i < weaponParent.childCount; i++)
                    {
                        weaponParent.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    weaponParent.transform.GetChild((int)rarity).gameObject.SetActive(true);
                }

                interactText = " to pickup weapon";
                break;
            case LootTypes.SECONDARYWEAPON:
                weapons.transform.GetChild(1).gameObject.SetActive(true);
                weapons.transform.GetChild(0).gameObject.SetActive(false);

                rand = weaponIndex < 0 ? Random.Range(0, weaponsHolder.SecondaryGuns.Count) : weaponIndex;
                weaponIndex = rand;

                currentGun = weaponsHolder.SecondaryGuns[rand];

                Transform weaponSecondaryParent = weapons.transform.GetChild(1).GetChild(rand);

                weaponSecondaryParent.gameObject.SetActive(true);

                if (currentGun.HasRarity)
                {
                    for (int i = 0; i < weaponSecondaryParent.childCount; i++)
                    {
                        weaponSecondaryParent.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    weaponSecondaryParent.transform.GetChild((int)rarity).gameObject.SetActive(true);
                }

                interactText = " to pickup weapon";
                break;
            case LootTypes.SMALLAMMO:
                ammoCrates.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case LootTypes.MEDIUMAMMO:
                ammoCrates.transform.GetChild(1).gameObject.SetActive(true);
                break;
            case LootTypes.LARGEAMMO:
                interactText = " to open ammo crate";
                ammoCrates.transform.GetChild(2).gameObject.SetActive(true);
                break;
            case LootTypes.HEALTH:
                items.transform.GetChild(0).gameObject.SetActive(true);
                break;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("UpdateLootForOthers", RpcTarget.Others, (byte)type, (byte)weaponIndex, (byte)rarity);
        }
    }

    private void DeactivateAllWeapons()
    {
        foreach (Transform child in weapons.transform.GetChild(0).GetComponentInChildren<Transform>())
        {
            child.gameObject.SetActive(false);
        }

        foreach (Transform child in weapons.transform.GetChild(1).GetComponentInChildren<Transform>())
        {
            child.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void UpdateLootForOthers(byte typeIndex, byte weaponIndex, byte rarityIndex)
    {
        UpdateLootType((LootTypes)typeIndex, weaponIndex, (LootRoomGenerator.Rarities)rarityIndex);
    }

    protected override void PlayerTriggerEntered(PlayerStats playerStats)
    {
        base.PlayerTriggerEntered(playerStats);

        if (lootType == LootTypes.SMALLAMMO)
        {
            playerStats.OnInteract = null;
            playerStats.PlayerShootScript.GiveAmmo(smallAmmoDrop);
            playerUI.HideInteractPanel();

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("DeactivateLootForOthers", RpcTarget.Others);
            }

            gameObject.SetActive(false);
        }
        else if (lootType == LootTypes.MEDIUMAMMO)
        {
            playerStats.OnInteract = null;
            playerStats.PlayerShootScript.GiveAmmo(mediumAmmoDrop);

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("DeactivateLootForOthers", RpcTarget.Others);
            }
            playerUI.HideInteractPanel();

            gameObject.SetActive(false);
        }
        else if (lootType == LootTypes.HEALTH)
        {
            playerStats.OnInteract = null;
            playerUI.HideInteractPanel();
            bool succeeded = playerStats.PlayerHealingScript.AddHealthItem();

            if (!succeeded)
            {
                return;
            }

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("DeactivateLootForOthers", RpcTarget.Others);
            }

            gameObject.SetActive(false);
        }
        else
        {
            playerStats.OnInteract += PlayerInteracted;
        }
    }

    [PunRPC]
    void DeactivateLootForOthers()
    {
        gameObject.SetActive(false);
    }

    private void PlayerInteracted(PlayerStats playerStats)
    {
        switch (lootType)
        {
            case LootTypes.PRIMARYWEAPON:
                SwapWeapon(playerStats.PlayerShootScript);
                break;
            case LootTypes.SECONDARYWEAPON:
                SwapWeapon(playerStats.PlayerShootScript);
                break;
            case LootTypes.LARGEAMMO:
                playerStats.PlayerShootScript.GiveFullAmmo(false);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("DeactivateLootForOthers", RpcTarget.Others);
                }

                gameObject.SetActive(false);
                break;
        }

        playerStats.OnInteract = null;
    }

    private void SwapWeapon(PlayerShoot playerShoot)
    {
        GunObject gun = playerShoot.CurrentGun;

        LootRoomGenerator.Rarities rarity = playerShoot.CurrentRarity;

        bool secondaryWasNull = playerShoot.SecondaryGun == null;

        playerShoot.GiveWeapon(currentGun, currentRarity, ammo);

        if (secondaryWasNull)
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("DeactivateLootForOthers", RpcTarget.Others);
            }                

            gameObject.SetActive(false);

            return;
        }

        currentRarity = rarity;
        UpdateCurrentGun(gun, currentRarity);
        ammo = playerShoot.CurrentAmmo;

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("UpdateCurrentGunForOthers", RpcTarget.Others, 
                (byte)weaponsHolder.SearchWeaponIndex(gun.name, gun.Primary), gun.Primary, (byte)rarity);
        }
    }

    private void UpdateCurrentGun(GunObject gun, LootRoomGenerator.Rarities rarity)
    {
        currentGun = gun;
        int weaponIndex = weaponsHolder.SearchWeaponIndex(currentGun.name, currentGun.Primary);
        currentRarity = rarity;

        DeactivateAllWeapons();

        if (currentGun.Primary)
        {
            weapons.transform.GetChild(0).gameObject.SetActive(true);
            weapons.transform.GetChild(1).gameObject.SetActive(false);
            weapons.transform.GetChild(0).GetChild(weaponIndex).gameObject.SetActive(true);

            Transform weaponParent = weapons.transform.GetChild(0).GetChild(weaponIndex);

            if (currentGun.HasRarity)
            {
                for (int i = 0; i < weaponParent.childCount; i++)
                {
                    weaponParent.transform.GetChild(i).gameObject.SetActive(false);
                }

                weaponParent.transform.GetChild((int)rarity).gameObject.SetActive(true);
            }
        }
        else
        {
            weapons.transform.GetChild(0).gameObject.SetActive(false);
            weapons.transform.GetChild(1).gameObject.SetActive(true);
            weapons.transform.GetChild(1).GetChild(weaponIndex).gameObject.SetActive(true);

            Transform weaponParent = weapons.transform.GetChild(1).GetChild(weaponIndex);

            if (currentGun.HasRarity)
            {
                for (int i = 0; i < weaponParent.childCount; i++)
                {
                    weaponParent.transform.GetChild(i).gameObject.SetActive(false);
                }

                weaponParent.transform.GetChild((int)rarity).gameObject.SetActive(true);
            }
        }
    }

    [PunRPC]
    void UpdateCurrentGunForOthers(byte weaponIndex, bool primary, byte rarityIndex)
    {
        UpdateCurrentGun(primary ? weaponsHolder.PrimaryGuns[weaponIndex] : 
            weaponsHolder.SecondaryGuns[weaponIndex], (LootRoomGenerator.Rarities)rarityIndex);
    }
}
