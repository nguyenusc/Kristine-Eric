using UnityEngine;
using UnityEngine.SceneManagement;

public class MetricManager : MonoBehaviour
{
    public static MetricManager Instance;

    void Awake()
    {
        // Singleton pattern: Ensure there's only one instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Subscribe to sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe when not needed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is called every time a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only start tracking if the loaded scene is "Level 1"
        if (scene.name == "Tuning Split Jump")
        {
            MetricsTracker.StartLevel(scene.name);
        }
    }
}
