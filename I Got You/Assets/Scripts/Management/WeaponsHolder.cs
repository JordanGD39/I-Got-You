using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsHolder : MonoBehaviour
{
    [SerializeField] private List<GunObject> primaryGuns = new List<GunObject>();
    public List<GunObject> PrimaryGuns { get { return primaryGuns; } }
    [SerializeField] private List<GunObject> secondaryGuns = new List<GunObject>();
    public List<GunObject> SecondaryGuns { get { return secondaryGuns; } }

    public int SearchWeaponIndex(string searchName, bool primary)
    {
        if (primary)
        {
            foreach (GunObject item in primaryGuns)
            {
                if (item.name == searchName)
                {
                    return primaryGuns.IndexOf(item);
                }
            }
        }
        else
        {
            foreach (GunObject item in secondaryGuns)
            {
                if (item.name == searchName)
                {
                    return secondaryGuns.IndexOf(item);
                }
            }
        }

        return -1;
    }
}
