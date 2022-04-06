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
    [SerializeField] private RectTransform hitMarker;
    [SerializeField] private RectTransform canvas;
    private Vector2 uiOffset;

    private void Start()
    {
        uiOffset = new Vector2(canvas.sizeDelta.x / 2f, canvas.sizeDelta.y / 2f);
    }

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
}
