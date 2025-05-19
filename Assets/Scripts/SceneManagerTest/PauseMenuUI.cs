using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    // Reference to the pause menu canvas (set this in the Inspector)
    public GameObject pauseMenuCanvas;

    // Optional: Reference to an Options Panel if you create one
    public GameObject optionsPanel;

    // Internal flag to track whether the game is paused
    private bool isPaused = false;

    void Update()
    {
        // Listen for the Esc key each frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If not paused, then pause the game
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                // If the options panel is active, close it first
                if (optionsPanel != null && optionsPanel.activeSelf)
                {
                    CloseOptions();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
    }

    // Pauses the game by showing the pause menu and freezing time
    public void PauseGame()
    {
        pauseMenuCanvas.SetActive(true);  // Show the pause menu UI
        Time.timeScale = 0f;                // Freeze game time
        isPaused = true;
    }

    // Resumes the game by hiding the pause menu and restoring time
    public void ResumeGame()
    {
        pauseMenuCanvas.SetActive(false); // Hide the pause menu UI
        Time.timeScale = 1f;                // Resume game time
        isPaused = false;
    }

    // Opens the Options panel (if you have one)
    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Options panel is not assigned.");
        }
    }

    // Closes the Options panel and returns to the main pause menu
    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    // Example function: Quit to Level Selection scene
    public void QuitToLevelSelect()
    {
        // Resume time before loading a new scene
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("LevelSelection");
    }

    // Example function: Quit the game application
    public void QuitGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Application.Quit();
        Debug.Log("QuitGame called - works in build (in Editor, it just logs this).");
    }
}
