using UnityEngine;
using UnityEngine.UI;      // for LayoutElement
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class CreditsRoll : MonoBehaviour
{
    [SerializeField] private float speed = 50f;
    [SerializeField] private float startY = -400f;
    [SerializeField] private float endY = 400f;

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        //----------------------------------------------------
        // 1) Make sure ANY parent layout group ignores us
        //----------------------------------------------------
        var le = GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    private void OnEnable()
    {
        //----------------------------------------------------
        // 2) Wait until the very end of the first frame,
        //    AFTER all layout calculations are done,
        //    then set the starting pos.
        //----------------------------------------------------
        StartCoroutine(SnapNextFrame());
    }

    private IEnumerator SnapNextFrame()
    {
        yield return new WaitForEndOfFrame();             // layout pass finished
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startY);
    }

    // -------------------------------------------------------
    // 3) Move in LateUpdate so we always run AFTER layout
    // -------------------------------------------------------
    private void LateUpdate()
    {
        rt.anchoredPosition += Vector2.up * speed * Time.unscaledDeltaTime;

        if (rt.anchoredPosition.y >= endY)
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startY);
    }
}
