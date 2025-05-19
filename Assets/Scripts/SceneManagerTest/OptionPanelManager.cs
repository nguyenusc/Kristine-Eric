// OptionPanelManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
public class OptionPanelManager : MonoBehaviour
{
    public static OptionPanelManager Instance { get; private set; }

    [SerializeField] private GameObject optionCanvas;  // root canvas

    private void Awake()
    {

        Instance = this;


    }

    // OptionPanelManager.cs
    public void Show()
    {
        optionCanvas.SetActive(true);
    }

    public void Hide()
    {
        optionCanvas.SetActive(false);
    }

    public void Toggle()
    {
        optionCanvas.SetActive(!optionCanvas.activeSelf);
    }

}
