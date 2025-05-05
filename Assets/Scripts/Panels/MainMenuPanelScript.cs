using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MainMenuPanelScript : MonoBehaviour
{
    [Header("UI Elements")]
    public Button mainMenuButton;
    public Button startButton;
    public Button configurationButton;
    public Button quitButton;

    void Start()
    {
        AudioManager.Instance.StopAllSFX();
        AudioManager.Instance.StopMusic();

        AudioClip menuMusic = Resources.Load<AudioClip>("Sounds/main_menu_song");
        if (menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(MainMenu);
            AddSoundToButton(mainMenuButton);
        }
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
            AddSoundToButton(startButton);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
            AddSoundToButton(quitButton);
        }
        if (configurationButton != null)
        {
            configurationButton.onClick.RemoveAllListeners();
            configurationButton.onClick.AddListener(ConfigureGame);
            AddSoundToButton(configurationButton);
        }
    }

    private void AddSoundToButton(Button button)
    {
        if (button != null)
        {
            var trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();
            else
                trigger.triggers.Clear();

            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entry.callback.AddListener((eventData) => AudioManager.Instance.PlayCursorSound());
            trigger.triggers.Add(entry);

            // Importante: evita acumulación de click
            button.onClick.RemoveListener(AudioManager.Instance.PlayClickSound);
            button.onClick.AddListener(AudioManager.Instance.PlayClickSound);
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ConfigureGame()
    {
        PanelManager.Instance.ShowPanel(MenuPanel.Configuration);
    }

    public void StartGame()
    {
        PanelManager.Instance.ShowPanel(MenuPanel.GameMode);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
