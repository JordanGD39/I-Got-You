using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Text healthText;
    [SerializeField] private Text maxHealthText;
    [SerializeField] private Text shieldHealthText;
    [SerializeField] private Animator shieldHealthAnim;
    [SerializeField] private Text maxShieldHealthText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text maxAmmoText;
    [SerializeField] private Text healthItemCount;
    [SerializeField] private Text maxHealthItemCount;
    [SerializeField] private GameObject healthItemPanel;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private RectTransform hitMarker;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private GameObject bloodScreen;
    [SerializeField] private GameObject errorPopup;
    [SerializeField] private GameObject chickenSoupBarPanel;
    [SerializeField] private Image chickenSoupBar;
    [SerializeField] private float[] healthArray = { 0.081f, 0.157f, 0.233f, 0.312f, 0.391f, 0.469f, 0.546f, 0.625f, 0.703f, 0.779f, 0.858f, 0.939f, 1f };
    //private Vector2 uiOffset;

    private int itemCount = 0;

    private void Start()
    {
        //uiOffset = new Vector2(canvas.sizeDelta.x / 2f, canvas.sizeDelta.y / 2f);
        bloodScreen.SetActive(false);
        errorPopup.SetActive(false);
    }

    public void UpdateHealth(float health, float maxHealth)
    {
        //healthText.text = health.ToString();
        float healthPercentage = health / maxHealth;

        if (health < 0.5)
        {
            //start searching from 0 in array
        }
        else
        {
            //start searching from 1 in array
        }

        for (int i = 0; i < healthArray.Length; i++)
        {

        }
    }

    //public void UpdateMaxHealth(float maxHealth)
    //{
        //maxHealthText.text = "/" + maxHealth.ToString();
    //}

    public void HideShieldHealth()
    {
        shieldHealthText.transform.parent.gameObject.SetActive(false);
    }

    public void UpdateShieldHealth(float health, int maxHealth)
    {
        shieldHealthAnim.SetBool("FadeOut", health == maxHealth);

        shieldHealthText.text = Mathf.RoundToInt(health).ToString();
    }

    public void UpdateMaxShieldHealth(int maxHealth)
    {
        maxShieldHealthText.text = "/" + maxHealth.ToString();
    }

    public void UpdateAmmo(int ammo, int maxAmmo)
    {
        ammoText.text = ammo.ToString();
        maxAmmoText.text = "/" + maxAmmo.ToString();
    }

    public void ShowHitMarker(Vector3 objectTransformPosition)
    {
        //// Get the position on the canvas
        //Vector2 viewportPosition = Camera.main.WorldToViewportPoint(objectTransformPosition);
        //Vector2 proportionalPosition = new Vector2(viewportPosition.x * canvas.sizeDelta.x, viewportPosition.y * canvas.sizeDelta.y);

        //// Set the position and remove the screen offset
        //hitMarker.localPosition = proportionalPosition - uiOffset;
        hitMarker.gameObject.SetActive(false);
        hitMarker.gameObject.SetActive(true);
    }

    public void UpdateRoundText(int round)
    {
        roundText.text = (round + 1).ToString();
    }

    public void ShowBloodScreen()
    {
        bloodScreen.SetActive(false);
        bloodScreen.SetActive(true);
    }

    public void ShowErrorScreen(string error)
    {
        errorText.text = error;
        errorPopup.gameObject.SetActive(true);
    }

    public void UpdateHealthItemCount(int count)
    {
        itemCount = count;
        healthItemPanel.SetActive(itemCount > 0);

        healthItemCount.text = itemCount.ToString();
    }

    public void UpdateMaxHealthItemCount(int count)
    {
        maxHealthItemCount.text = "/" + count.ToString();
    }

    public void UpdateChickenSoupBar(float currentAmount, float maxAmount)
    {
        healthItemPanel.SetActive(false);
        chickenSoupBarPanel.SetActive(true);

        chickenSoupBar.fillAmount = currentAmount / maxAmount;
    }

    public void HideChickenSoupBar()
    {
        chickenSoupBarPanel.SetActive(false);
        healthItemPanel.SetActive(itemCount > 0);
    }
}
