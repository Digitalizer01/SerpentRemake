using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameOverMenuPanelScript : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text gameOverText;
    public Button mainMenuButton;
    public Button quitButton;

    void OnEnable()
    {
        AudioManager.Instance.StopMusic();

        AudioClip winClip = Resources.Load<AudioClip>("Sounds/round_victory_song");
        AudioClip loseClip = Resources.Load<AudioClip>("Sounds/round_defeat_song");

        if (GameResultInfo.PlayerWon)
        {
            gameOverText.text = "You win!";
            if (winClip != null)
                AudioManager.Instance.PlaySFX(winClip);
        }
        else
        {
            gameOverText.text = "You lose";
            if (loseClip != null)
                AudioManager.Instance.PlaySFX(loseClip);
        }
        if(mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(MainMenu);
            AddSoundToButton(mainMenuButton);
        }
        if(quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
            AddSoundToButton(quitButton);
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

            button.onClick.RemoveListener(AudioManager.Instance.PlayClickSound);
            button.onClick.AddListener(AudioManager.Instance.PlayClickSound);
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
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
