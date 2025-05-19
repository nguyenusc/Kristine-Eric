using UnityEngine;
using UnityEngine.SceneManagement;  // needed to use SceneManager

public class MainMenuUI : MonoBehaviour
{
    // This method will be called when "Start Game" is clicked
    public void StartGame()
    {
        // Load the Level Selection scene (make sure the name matches exactly)
        SceneManager.LoadScene("LevelSelection");
    }

    // This will be called for the Options button (not implemented yet)
    public void OpenOptions()
    {
        // For now, just log or do nothing. We can implement options later.
        Debug.Log("Options menu clicked (not implemented yet).");
        // Optionally, you could show an options panel if it existed.
    }

    // This will be called when "Exit Game" is clicked
    public void QuitGame()
    {
        Application.Quit();
        // Note: Application.Quit() is ignored in the Unity Editor&#8203;:contentReference[oaicite:2]{index=2}.
        // It will work in a build (closing the application). In editor, use Debug to verify.
        Debug.Log("QuitGame called - if this were a build, the game would close.");
    }
}
