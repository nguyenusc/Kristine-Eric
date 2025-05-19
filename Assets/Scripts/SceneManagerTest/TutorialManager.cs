using UnityEngine;

/// <summary>
/// Central hub that shows / hides tutorial panels and pauses the game.
/// Put this on a single GameObject in the scene.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Global toggle")]
    public bool enableTutorial = true;

    [Header("Dismiss key")]
    public KeyCode dismissKey = KeyCode.C;   // ← new, editable in Inspector

    [Header("Canvas & Panels")]
    public Canvas tutorialCanvas;
    public GameObject[] panels;

    private bool waitingForSpace;
    private bool ignoreSpaceThisFrame;


    private void Awake()
    {
        // If tutorials are disabled, hide everything & disable triggers.
        if (!enableTutorial)
        {
            if (tutorialCanvas) tutorialCanvas.gameObject.SetActive(false);
            foreach (var trig in FindObjectsOfType<TutorialTrigger>())
                trig.gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        if (waitingForSpace && Input.GetKeyDown(dismissKey))
        {
            ClosePanel();
            ignoreSpaceThisFrame = true;   // set flag for this frame
            return;                       // skip rest of Update
        }

        if (ignoreSpaceThisFrame)
        {
            // eat the lingering key‑down event
            ignoreSpaceThisFrame = false;
            return;
        }
    }


    /// <summary>Called by TutorialTrigger.</summary>
    public void ShowPanel(int index)
    {
        if (!enableTutorial || waitingForSpace) return;
        if (index < 0 || index >= panels.Length)
        {
            Debug.LogWarning($"No panel at index {index}");
            return;
        }

        // Activate canvas & the chosen panel, hide the rest
        tutorialCanvas.gameObject.SetActive(true);
        for (int i = 0; i < panels.Length; ++i)
            panels[i].SetActive(i == index);

        Time.timeScale = 0f;    // pause game
        waitingForSpace = true;
    }

    private void ClosePanel()
    {

        foreach (var p in panels) p.SetActive(false);
        Input.ResetInputAxes();         // ← FLUSH any pending key presses
        Time.timeScale = 1f;    // resume
        waitingForSpace = false;
    }
}
