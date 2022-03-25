using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Text healthText;
    [SerializeField] private Text maxHealthText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text maxAmmoText;

    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();        
    }

    public void UpdateMaxHealth(int maxHealth)
    {
        maxHealthText.text = "/" + maxHealth.ToString();
    }

    public void UpdateAmmo(int ammo, int maxAmmo)
    {
        ammoText.text = ammo.ToString();
        maxAmmoText.text = "/" + maxAmmo.ToString();
    }
}
