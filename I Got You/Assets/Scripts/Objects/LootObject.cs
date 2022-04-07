using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LootObject : MonoBehaviourPun
{
    public enum LootTypes { PRIMARYWEAPON, SECONDARYWEAPON, SMALLAMMO, MEDIUMAMMO, LARGEAMMO }
    [SerializeField] private LootTypes lootType = LootTypes.SMALLAMMO;

    [SerializeField] private GameObject ammoCrates;
    [SerializeField] private GameObject weapons;
    [SerializeField] private int smallAmmoDrop = 50;
    [SerializeField] private int mediumAmmoDrop = 100;
    private WeaponsHolder weaponsHolder;
    private PlayerManager playerManager;

    public GunObject currentGun { get; private set; }

    public void UpdateLootType(LootTypes type, int weaponIndex)
    {
        if (weaponsHolder == null)
        {
            weaponsHolder = FindObjectOfType<WeaponsHolder>();
            playerManager = FindObjectOfType<PlayerManager>();
        }

        lootType = type;

        if (type == LootTypes.PRIMARYWEAPON || type == LootTypes.SECONDARYWEAPON)
        {
            weapons.SetActive(true);
            ammoCrates.SetActive(false);

            DeactivateAllWeapons();
        }
        else
        {
            weapons.SetActive(false);
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

                weapons.transform.GetChild(0).GetChild(rand).gameObject.SetActive(true);
                break;
            case LootTypes.SECONDARYWEAPON:
                weapons.transform.GetChild(1).gameObject.SetActive(true);
                weapons.transform.GetChild(0).gameObject.SetActive(false);

                rand = weaponIndex < 0 ? Random.Range(0, weaponsHolder.SecondaryGuns.Count) : weaponIndex;
                weaponIndex = rand;

                currentGun = weaponsHolder.SecondaryGuns[rand];

                weapons.transform.GetChild(1).GetChild(rand).gameObject.SetActive(true);
                break;
            case LootTypes.SMALLAMMO:
                ammoCrates.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case LootTypes.MEDIUMAMMO:
                ammoCrates.transform.GetChild(1).gameObject.SetActive(true);
                break;
            case LootTypes.LARGEAMMO:
                ammoCrates.transform.GetChild(2).gameObject.SetActive(true);
                break;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("UpdateLootForOthers", RpcTarget.Others, (byte)type, (byte)weaponIndex);
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
    void UpdateLootForOthers(byte typeIndex, byte weaponIndex)
    {
        UpdateLootType((LootTypes)typeIndex, weaponIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            if (lootType == LootTypes.SMALLAMMO)
            {
                playerManager.StatsOfAllPlayers[other].PlayerShootScript.GiveAmmo(smallAmmoDrop);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("DeactivateLootForOthers", RpcTarget.Others);
                }

                gameObject.SetActive(false);
            }
            else if (lootType == LootTypes.MEDIUMAMMO)
            {
                playerManager.StatsOfAllPlayers[other].PlayerShootScript.GiveAmmo(mediumAmmoDrop);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("DeactivateLootForOthers", RpcTarget.Others);
                }

                gameObject.SetActive(false);
            }
            else
            {
                playerManager.StatsOfAllPlayers[other].OnInteract += PlayerInteracted;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol") && lootType != LootTypes.SMALLAMMO && lootType != LootTypes.MEDIUMAMMO)
        {
            PlayerStats playerStats = playerManager.StatsOfAllPlayers[other];

            if (playerStats.OnInteract != null)
            {
                playerStats.OnInteract -= PlayerInteracted;
            }
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

        playerStats.OnInteract -= PlayerInteracted;
    }

    private void SwapWeapon(PlayerShoot playerShoot)
    {
        GunObject gun = playerShoot.CurrentGun;

        bool secondaryWasNull = playerShoot.SecondaryGun == null;

        playerShoot.GiveWeapon(currentGun);

        if (secondaryWasNull)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateCurrentGun(gun);

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("UpdateCurrentGunForOthers", RpcTarget.Others, (byte)weaponsHolder.SearchWeaponIndex(gun.name, gun.Primary), gun.Primary);
        }
    }

    private void UpdateCurrentGun(GunObject gun)
    {
        currentGun = gun;
        int weaponIndex = weaponsHolder.SearchWeaponIndex(currentGun.name, currentGun.Primary);

        DeactivateAllWeapons();

        if (currentGun.Primary)
        {
            weapons.transform.GetChild(0).gameObject.SetActive(true);
            weapons.transform.GetChild(1).gameObject.SetActive(false);
            weapons.transform.GetChild(0).GetChild(weaponIndex).gameObject.SetActive(true);
        }
        else
        {
            weapons.transform.GetChild(0).gameObject.SetActive(false);
            weapons.transform.GetChild(1).gameObject.SetActive(true);
            weapons.transform.GetChild(1).GetChild(weaponIndex).gameObject.SetActive(true);
        }
    }

    [PunRPC]
    void UpdateCurrentGunForOthers(byte weaponIndex, bool primary)
    {
        UpdateCurrentGun(primary ? weaponsHolder.PrimaryGuns[weaponIndex] : weaponsHolder.SecondaryGuns[weaponIndex]);
    }
}
