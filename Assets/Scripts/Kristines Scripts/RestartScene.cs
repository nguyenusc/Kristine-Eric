using UnityEngine;
using UnityEngine.SceneManagement;


public class RestartScene : MonoBehaviour
{
    public void Restart()
    {
        Debug.Log("gameRestart");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
