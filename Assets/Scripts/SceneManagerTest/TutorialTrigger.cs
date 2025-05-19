using UnityEngine;

/// <summary>
/// Attach this to a trigger collider positioned on the track.
/// When the Player enters, it requests the panel from TutorialManager.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    public int panelIndex = 0;                 // which panel to show
    public TutorialManager manager;            // optional manual link

    private void Start()
    {
        // Find manager automatically if not assigned
        if (manager == null) manager = FindObjectOfType<TutorialManager>();
        // Ensure this collider is a trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("activate tutorial");
        manager.ShowPanel(panelIndex);
        gameObject.SetActive(false);   // fire once
    }


}
