using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Animator shieldHealthAnim;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text maxAmmoText;
    [SerializeField] private Text healthItemCount;
    [SerializeField] private Text maxHealthItemCount;
    [SerializeField] private GameObject healthItemPanel;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private Image hitMarker;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private GameObject bloodScreen;
    [SerializeField] private GameObject errorPopup;
    [SerializeField] private GameObject chickenSoupBarPanel;
    [SerializeField] private Image chickenSoupBar;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image shieldHealthBar;
    [SerializeField] private float[] healthArray = {0, 0.081f, 0.157f, 0.233f, 0.312f, 0.391f, 0.469f, 0.546f, 0.625f, 0.703f, 0.779f, 0.858f, 0.939f, 1f };
    [SerializeField] private GameObject interactPanel;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private Image circleRevive;
    [SerializeField] private Image[] abilities;
    [SerializeField] private TextMeshProUGUI abilityText;
    //private Vector2 uiOffset;

    private int itemCount = 0;
    private bool usingController = false;

    private void Start()
    {
        //uiOffset = new Vector2(canvas.sizeDelta.x / 2f, canvas.sizeDelta.y / 2f);
        bloodScreen.SetActive(false);
        errorPopup.SetActive(false);
        interactPanel.SetActive(false);
        circleRevive.gameObject.SetActive(false);
        abilityText.gameObject.SetActive(false);

        foreach (Image image in abilities)
        {
            image.transform.parent.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckControllerUsage();
    }

    private void CheckControllerUsage()
    {
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            usingController = !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S);
        }
        else if (Input.GetButtonDown("Interact"))
        {
            usingController = !Input.GetKeyDown(KeyCode.E);
        }
    }

    public void UpdateHealth(float health, float maxHealth)
    {
        //healthText.text = health.ToString();
        float healthPercentage = health / maxHealth;
        bool reverseSearch = false;

        if (healthPercentage > 0.5f)
        {
            //start searching from 0 in array
            reverseSearch = true;
        }

        float promisingHealthValue = 0;
        float lowestDiff = 2;

        int j = 0;

        for (int i = 0; i < healthArray.Length; i++)
        {
            j = i;

            if (reverseSearch)
            {
                j = healthArray.Length - 1 - i;
            }

            float diff = Mathf.Abs(healthPercentage - healthArray[j]);

            if (diff > lowestDiff && lowestDiff < 0.2f)
            {
                break;
            }

            if (diff < lowestDiff)
            {
                lowestDiff = diff;
                promisingHealthValue = healthArray[j];
            }
        }

        Debug.Log(lowestDiff);
        healthBar.fillAmount = promisingHealthValue;
    }

    //public void UpdateMaxHealth(float maxHealth)
    //{
        //maxHealthText.text = "/" + maxHealth.ToString();
    //}

    public void HideShieldHealth()
    {
        shieldHealthAnim.gameObject.SetActive(false);
    }

    public void UpdateShieldHealth(float health, int maxHealth)
    {
        shieldHealthAnim.SetBool("FadeOut", health == maxHealth);

        shieldHealthBar.fillAmount = health / (float)maxHealth;
    }

    public void UpdateAmmo(int ammo, int maxAmmo)
    {
        ammoText.text = ammo.ToString();
        maxAmmoText.text = "/" + maxAmmo.ToString();
    }

    public void ShowInteractPanel(string extraText)
    {
        interactPanel.SetActive(true);

        string interactButton = usingController ? "X" : "E";

        interactText.text = "Press " + interactButton + extraText; 
    }

    public void ShowNotification(string text)
    {
        interactPanel.SetActive(true);

        interactText.text = text;
    }

    public void HideInteractPanel()
    {
        interactPanel.SetActive(false);
    }

    public void UpdateAbility(int abilityIndex, float currentCharge, float maxCharge)
    {
        Image image = abilities[abilityIndex];
        image.transform.parent.gameObject.SetActive(true);
        image.fillAmount = currentCharge / maxCharge;

        string abilityButton = usingController ? "R1" : "Q";

        abilityText.text = abilityButton;
        abilityText.gameObject.SetActive(currentCharge >= maxCharge);
    }

    public void ShowHitMarker(Vector3 objectTransformPosition, bool weakSpot)
    {
        //// Get the position on the canvas
        //Vector2 viewportPosition = Camera.main.WorldToViewportPoint(objectTransformPosition);
        //Vector2 proportionalPosition = new Vector2(viewportPosition.x * canvas.sizeDelta.x, viewportPosition.y * canvas.sizeDelta.y);

        //// Set the position and remove the screen offset
        //hitMarker.localPosition = proportionalPosition - uiOffset;
        hitMarker.gameObject.SetActive(false);
        hitMarker.gameObject.SetActive(true);
        Color color = weakSpot ? Color.red : Color.white;
        color.a = hitMarker.color.a;
        hitMarker.color = color;
    }

    //public void UpdateRoundText(int round)
    //{
    //    roundText.text = (round + 1).ToString();
    //}

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

    public void StartReviveTimer(float reviveTime)
    {
        circleRevive.gameObject.SetActive(true);
        StartCoroutine(CountDownRevive(reviveTime));
    }

    private IEnumerator CountDownRevive(float reviveTime)
    {
        float startTime = Time.time;
        Vector3 startPos = transform.position;

        float frac = 0;

        while (frac < 1)
        {
            frac = (Time.time - startTime) / reviveTime;

            circleRevive.fillAmount = Mathf.Lerp(1, 0, frac);

            yield return null;
        }
    }

    public void StopReviveTimer()
    {
        StopCoroutine(CountDownRevive(0));
        circleRevive.gameObject.SetActive(false);
    }
}
