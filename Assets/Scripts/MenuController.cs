using UnityEngine;
using UnityEngine.SceneManagement;  // Needed to load scenes
using UnityEngine.UI;  // Needed to work with UI buttons
using TMPro;

public class MenuController : MonoBehaviour
{
    // Assign this script to the object that contains the buttons
    public TMP_Text gameOverText;
    public Button mainMenuButton;
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        Screen.SetResolution(500, 500, true);

        // Attach the functions to the buttons when the scene starts
        if(mainMenuButton != null) mainMenuButton.onClick.AddListener(MainMenu);
        if(startButton != null) startButton.onClick.AddListener(StartGame);
        if(quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if(gameOverText != null)
        {
            if(GameResultInfo.PlayerWon)
            {
                gameOverText.text = "You win!";
            }
            else
            {
                gameOverText.text = "You lose...";
            }
        }
    }

    // Method to start the game and load a new scene
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");  // Change "MainMenu" to your actual scene name
    }

    public void StartGame()
    {
        // Here you can set the name of the scene you want to load
        SceneManager.LoadScene("GameScene");  // Change "GameScene" to your actual scene name
    }

    // Method to quit the application
    public void QuitGame()
    {
        // If we are in the Unity editor, stop the game
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If we are in a built game, quit the application
        Application.Quit();
        #endif
    }
}
