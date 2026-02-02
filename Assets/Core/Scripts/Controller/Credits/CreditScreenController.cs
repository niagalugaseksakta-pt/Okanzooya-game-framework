using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CreditSceneController : MonoBehaviour
{
    [Header("=== Credits ===")]
    [TextArea(2, 5)]
    public List<string> creditLines = new List<string>
    {
        "Director: Ginanjar Rahman",
        "Lead Programmer: Vega Orion",
        "Art Director: Lyra Pixelstorm",
        "Sound Designer: Kian Moon",
        "Special Thanks: The ChatGPT Beast"
    };

    [Header("=== Text UI ===")]
    public TextMeshProUGUI creditText; // assign from Canvas (UI)
    public RectTransform cameraRect;   // optional: for UI group movement
    public float moveSpeed = 30f;      // pixels/sec
    public float moveAmplitude = 100f; // how far it moves side-to-side

    [Header("=== Timing ===")]
    public float fadeInTime = 1.2f;
    public float holdTime = 2.5f;
    public float fadeOutTime = 1.2f;
    public float directionChangeDelay = 4f;

    private int currentIndex = 0;
    private bool movingRight = true;
    private float baseX;

    private void Start()
    {
        if (!creditText)
        {
            Debug.LogError("[SimpleCreditsController] Missing TextMeshProUGUI reference!");
            enabled = false;
            return;
        }

        if (!cameraRect)
            cameraRect = creditText.GetComponent<RectTransform>();

        baseX = cameraRect.anchoredPosition.x;

        StartCoroutine(CreditSequence());
        StartCoroutine(MoveCameraUI());
    }

    // --- Horizontal camera-like motion (UI panning) ---
    private IEnumerator MoveCameraUI()
    {
        while (true)
        {
            float dir = movingRight ? 1f : -1f;
            float newX = cameraRect.anchoredPosition.x + dir * moveSpeed * Time.deltaTime;

            // Bounce when reaching limits
            if (Mathf.Abs(newX - baseX) >= moveAmplitude)
            {
                movingRight = !movingRight;
                yield return new WaitForSeconds(directionChangeDelay);
            }

            cameraRect.anchoredPosition = new Vector2(newX, cameraRect.anchoredPosition.y);
            yield return null;
        }
    }

    // --- Text fading and transitions ---
    private IEnumerator CreditSequence()
    {
        while (true)
        {
            string text = creditLines[currentIndex];
            yield return StartCoroutine(FadeText(text));

            currentIndex = (currentIndex + 1) % creditLines.Count;
        }
    }

    private IEnumerator FadeText(string newText)
    {
        creditText.text = newText;

        // Fade in
        float t = 0;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            creditText.alpha = Mathf.Lerp(0, 1, t / fadeInTime);
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        // Fade out
        t = 0;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            creditText.alpha = Mathf.Lerp(1, 0, t / fadeOutTime);
            yield return null;
        }
    }
}
