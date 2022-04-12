using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputUI : MonoBehaviour
{
    [SerializeField] private SettingsUI settingsPanel;
    public SettingsUI SettingsUI { get { return settingsPanel; } }

    public delegate void TogglePausedGame();
    public TogglePausedGame OnTogglePausedGame;

    private void Start()
    {
        settingsPanel.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            settingsPanel.gameObject.SetActive(!settingsPanel.gameObject.activeSelf);
            OnTogglePausedGame?.Invoke();
        }
    }
}
