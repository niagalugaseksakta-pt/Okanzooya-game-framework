using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasTouchUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private CanvasGroup targetCanvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private bool hasShown = false;
    [SerializeField] private bool autoShow = true;
    [SerializeField] private float timeAutoShow = 0.05f;

    private void Update()
    {
        // Touch (Mobile)
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TryShow();
        }

        // Mouse (Editor / Desktop)
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryShow();
        }
        // Mouse (Editor / Desktop)
        if (autoShow)
        {
            TryShow();
        }
    }

    private void TryShow()
    {
        if (hasShown) return;

        hasShown = true;
        StartCoroutine(FadeIn());
        autoShow = false;
    }

    private IEnumerator FadeIn()
    {
        if (autoShow)
        {
            yield return new WaitForSeconds(timeAutoShow);
        }

        targetCanvasGroup.gameObject.SetActive(true);

        float time = 0f;
        targetCanvasGroup.alpha = 0f;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            targetCanvasGroup.alpha = time / fadeDuration;
            yield return null;
        }

        targetCanvasGroup.alpha = 1f;
        targetCanvasGroup.interactable = true;
        targetCanvasGroup.blocksRaycasts = true;
    }
}
