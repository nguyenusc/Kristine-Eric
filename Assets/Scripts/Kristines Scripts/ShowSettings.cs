using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShowSettings : MonoBehaviour
{
    [SerializeField] GameObject settingsPanel;

    // Set the state of the panel depending on the opposite state of the panel
    public void TogglePanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
}
