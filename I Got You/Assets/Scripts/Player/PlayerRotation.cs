using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 2;
    [SerializeField] private float downedAdder = -50;
    [SerializeField] private float lerpSpeed = 2;
    [SerializeField] private float minLerpDiff = 1;
    private float xRotation = 0;
    private bool lerp = false;
    private bool stopRotation = false;
    private PlayerStats playerStats;

    [SerializeField] private Transform cam;

    public delegate void GiveInputBack();
    public GiveInputBack OnGiveInputBack;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InputUI inputUI = FindObjectOfType<InputUI>();

        inputUI.OnTogglePausedGame += () => 
        {
            stopRotation = !stopRotation;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        };

        inputUI.SettingsUI.OnChangedSensitivity += ChangeSensitivity;

        ChangeSensitivity(PlayerPrefs.GetInt("Sensitivity", 6));
    }

    private void ChangeSensitivity(int val)
    {
        rotationSpeed = (float)val * 100 / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (stopRotation)
        {
            return;
        }

        if (!lerp)
        {
            UpdateRotation();
        }
        else
        {
            LerpToResetPos();
        }
    }

    private void LerpToResetPos()
    {
        cam.localRotation = Quaternion.Lerp(cam.localRotation, Quaternion.Euler(downedAdder, 0, 0), lerpSpeed * Time.deltaTime);

        if (Mathf.Abs(cam.localEulerAngles.x - downedAdder) < minLerpDiff)
        {
            xRotation = 0;
            lerp = false;
            OnGiveInputBack?.Invoke();
        }
    }

    private void UpdateRotation()
    {
        Vector2 input = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        input *= rotationSpeed * Time.deltaTime;

        xRotation -= input.y;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        cam.localRotation = Quaternion.Euler(xRotation + (playerStats.IsDown ? downedAdder : 0), 0, 0);

        transform.Rotate(Vector3.up * input.x);
    }

    public void StartLerpToResetPos()
    {
        lerp = true;
    }
}
