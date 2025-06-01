using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameSelectInputPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text gameModeText;
    public TMP_Text player2Text;
    public Image inputPlayer1Image;
    public Image inputPlayer2Image;
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.yellow;

    public Button startGameButton;
    public Button backButton;

    void OnEnable()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackPresssed);

        string gameMode = PlayerPrefs.GetString("GameMode", "PlayerVsCPU");

        if (gameMode == "PlayerVsCPU")
        {
            GameResultInfo.IsTwoPlayerMode = false;
            gameModeText.text = "Player vs CPU";
            player2Text.text = "CPU";
            if (inputPlayer2Image != null)
                inputPlayer2Image.gameObject.SetActive(false);
        }
        else if (gameMode == "PlayerVsPlayer")
        {
            GameResultInfo.IsTwoPlayerMode = true;
            gameModeText.text = "Player vs Player";
            player2Text.text = "Player 2";
            if (inputPlayer2Image != null)
                inputPlayer2Image.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        float dpadH1 = Input.GetAxisRaw("DPad1Horizontal");
        float dpadV1 = Input.GetAxisRaw("DPad1Vertical");
        bool isPlayer1PressingDPad = Mathf.Abs(dpadH1) > 0.1f || Mathf.Abs(dpadV1) > 0.1f;

        float dpadH2 = Input.GetAxisRaw("DPad2Horizontal");
        float dpadV2 = Input.GetAxisRaw("DPad2Vertical");
        bool isPlayer2PressingDPad = Mathf.Abs(dpadH2) > 0.1f || Mathf.Abs(dpadV2) > 0.1f;

        if (inputPlayer1Image != null)
            inputPlayer1Image.color = isPlayer1PressingDPad ? highlightedColor : normalColor;

        if (inputPlayer2Image != null)
            inputPlayer2Image.color = isPlayer2PressingDPad ? highlightedColor : normalColor;
    }

    private void OnBackPresssed()
    {
        PanelManager.Instance.GoBack();
    }

    private void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
