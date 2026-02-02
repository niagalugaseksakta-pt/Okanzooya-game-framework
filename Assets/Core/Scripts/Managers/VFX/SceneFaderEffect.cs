using Game.Config;
using System.Collections;
using UnityEngine;
public class SceneFaderEffect : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void RestartFade()
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("SceneFader: canvasGroup is not assigned.");
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        // Start fully opaque so fade-in works consistently
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(Config.cameraWaitToplayer + 1f);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            yield return null;
        }

        canvasGroup.alpha = 0f;
        fadeCoroutine = null;
    }
}