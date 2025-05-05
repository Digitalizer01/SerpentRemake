using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameModePanelScript : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playerVsCpuButton;
    public Button playerVsPlayerButton;
    public Button backButton;

    void OnEnable(){
        if (playerVsCpuButton != null)
        {
            playerVsCpuButton.onClick.AddListener(PlayerVSCpuMode);
        }
        if (playerVsPlayerButton != null)
        {
            playerVsPlayerButton.onClick.AddListener(PlayerVSPlayerMode);
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackPresssed);
        }
    }

    void PlayerVSCpuMode()
    {
        PlayerPrefs.SetString("GameMode", "PlayerVsCPU");
        PlayerPrefs.Save();
        PanelManager.Instance.ShowPanel(MenuPanel.GameSelectInput);
    }

    void PlayerVSPlayerMode()
    {
        PlayerPrefs.SetString("GameMode", "PlayerVsPlayer");
        PlayerPrefs.Save();
        PanelManager.Instance.ShowPanel(MenuPanel.GameSelectInput);
    }

    private void OnBackPresssed()
    {
        PanelManager.Instance.GoBack();
    }
}
