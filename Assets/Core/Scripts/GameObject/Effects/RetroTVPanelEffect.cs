using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RetroTVPanelEffect : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image panelImage;
    [SerializeField] private RectTransform[] slideImages;

    [Header("Timings")]
    [SerializeField] private float slideDistance = 600f;
    [SerializeField] private float slideDuration = 2.2f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float holdTime = 0.5f;

    [Header("Motion Style")]
    [SerializeField] private Ease slideEase = Ease.InOutSine;
    [SerializeField] private Ease returnEase = Ease.OutExpo;
    [SerializeField] private float overshoot = 40f; // tiny bounce feel
    [SerializeField] private float blurPulse = 0.15f; // fake “scanline pulse” intensity

    private Vector2[] startPositions;
    private Sequence seq;

    void Awake()
    {
        if (slideImages != null && slideImages.Length > 0)
        {
            startPositions = new Vector2[slideImages.Length];
            for (int i = 0; i < slideImages.Length; i++)
            {
                startPositions[i] = slideImages[i].anchoredPosition;
            }
        }

        if (panelImage != null)
            panelImage.canvasRenderer.SetAlpha(0f);
    }

    public void PlayEffect()
    {
        if (panelImage == null) return;

        seq?.Kill();
        seq = DOTween.Sequence();

        // Reset all image positions
        for (int i = 0; i < slideImages.Length; i++)
            slideImages[i].anchoredPosition = startPositions[i];

        panelImage.canvasRenderer.SetAlpha(1f);

        // --- FAKE SCANLINE PULSE ---
        seq.Append(DOTween.To(() => 0f, t =>
        {
            float pulse = Mathf.Sin(Time.time * 20f) * blurPulse;
            panelImage.material?.SetFloat("_BlurSize", pulse);
        }, 1f, 0.8f));

        // --- MAIN SLIDE MOTION ---
        for (int i = 0; i < slideImages.Length; i++)
        {
            var img = slideImages[i];
            seq.Join(img.DOAnchorPosX(startPositions[i].x + slideDistance + Random.Range(0, overshoot), slideDuration)
                .SetEase(slideEase)
                .OnUpdate(() =>
                {
                    // slight parallax feeling
                    float offset = Mathf.Sin(Time.time * 2f + i) * 2f;
                    img.localRotation = Quaternion.Euler(0, 0, offset);
                })
            );
        }

        // --- FADE OUT ALL ---
        seq.AppendInterval(holdTime);
        seq.Append(panelImage.DOFade(0f, fadeDuration).SetEase(Ease.InCubic));

        // --- RETURN IMAGES TO START ---
        seq.Join(DOVirtual.DelayedCall(fadeDuration * 0.8f, () =>
        {
            for (int i = 0; i < slideImages.Length; i++)
            {
                slideImages[i].DOAnchorPos(startPositions[i], fadeDuration)
                    .SetEase(returnEase);
            }
        }));

        // --- FINAL CLEANUP ---
        seq.OnComplete(() =>
        {
            for (int i = 0; i < slideImages.Length; i++)
                slideImages[i].anchoredPosition = startPositions[i];

            panelImage.canvasRenderer.SetAlpha(0f);
        });

        seq.Play();
    }
}
