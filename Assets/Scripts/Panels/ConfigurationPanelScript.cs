using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ConfigurationPanelScript : MonoBehaviour
{

    [Header("UI Elements")]
    public TMP_Dropdown resolutionsDropdown;
    public Slider volumeSlider;
    public Button backButton;

    private List<Vector2Int> resolutionOptions = new()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    private const string ResolutionPrefKey = "ScreenResolution";
    private const string VolumePrefKey = "MasterVolume";

    void OnEnable()
    {
        if (resolutionsDropdown != null)
        {
            PopulateResolutionDropdown();
            ApplySavedResolution();
        }

        if (volumeSlider != null)
        {
            InitializeVolumeSlider();
        }
        if(backButton != null)
        {
            backButton.onClick.AddListener(OnBackPresssed);
        }
    }

    #region Resolution

    void PopulateResolutionDropdown()
    {
        resolutionsDropdown.ClearOptions();
        List<string> options = new();
        int savedIndex = 0;

        string savedRes = PlayerPrefs.GetString(ResolutionPrefKey, "");

        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            Vector2Int res = resolutionOptions[i];
            string option = $"{res.x} x {res.y}";
            options.Add(option);

            if (option == savedRes)
            {
                savedIndex = i;
            }
        }

        resolutionsDropdown.AddOptions(options);
        resolutionsDropdown.value = savedIndex;
        resolutionsDropdown.onValueChanged.AddListener(SetResolution);
    }

    void ApplySavedResolution()
    {
        string savedRes = PlayerPrefs.GetString(ResolutionPrefKey, "");

        if (!string.IsNullOrEmpty(savedRes))
        {
            string[] parts = savedRes.Split('x');
            if (parts.Length == 2 &&
                int.TryParse(parts[0].Trim(), out int width) &&
                int.TryParse(parts[1].Trim(), out int height))
            {
                Screen.SetResolution(width, height, true);
            }
        }
        else
        {
            Vector2Int defaultRes = resolutionOptions[0];
            Screen.SetResolution(defaultRes.x, defaultRes.y, true);
        }
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < resolutionOptions.Count)
        {
            Vector2Int selectedRes = resolutionOptions[index];
            Screen.SetResolution(selectedRes.x, selectedRes.y, true);

            string resString = $"{selectedRes.x} x {selectedRes.y}";
            PlayerPrefs.SetString(ResolutionPrefKey, resString);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region Volume

    void InitializeVolumeSlider()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);  // Default volume = 1
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
        PlayerPrefs.Save();
        AudioManager.Instance.SetMusicVolume(volume);
        AudioManager.Instance.SetSFXVolume(volume);
    }

    #endregion

    private void OnBackPresssed()
    {
        PanelManager.Instance.GoBack();
    }
}
