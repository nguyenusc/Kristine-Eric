using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    public void PlayLevel1()
    {
        // Load the Level 1 scene
        SceneManager.LoadScene("UI change");
    }

    public void PlayLevel2()
    {
        // Level 2 is locked for now. We could show a message.
        Debug.Log("Level 2 is currently locked!");
        // (In future, this could load Level2 when unlocked)
    }

    public void BackToMainMenu()
    {
        // (Optional) If you want a back button to main menu, implement it.
        SceneManager.LoadScene("StartMenu");
    }
}
