using UnityEngine;
using UnityEngine.SceneManagement; // Provides access to scene management functions
using System.Collections;

public class SceneController : MonoBehaviour
{
    // This function loads a scene by its name synchronously.
    public void LoadScene(string sceneName)
    {
        // Optionally, add code here to perform a fade-out effect before switching scenes.
        SceneManager.LoadScene(sceneName);
        // Optionally, after loading, add code for a fade-in effect.
    }

    // This function loads a scene asynchronously with an optional loading screen.
    public void LoadSceneAsync(string sceneName)
    {
        // Start a coroutine to handle asynchronous loading.
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    // Coroutine for asynchronous scene loading.
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        // Begin loading the scene asynchronously.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Optionally, display a loading screen or progress bar.
        while (!asyncLoad.isDone)
        {
            // You can access the loading progress via asyncLoad.progress.
            // For example, update a UI progress bar here.
            yield return null; // Wait until the next frame.
        }
    }
}
