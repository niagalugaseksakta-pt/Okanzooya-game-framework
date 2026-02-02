using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static Game.Config.Config;

public class SplashScreenController : MonoBehaviour
{
    [Header("Video Control")]
    [SerializeField] private VideoPlayer player;
    [SerializeField] private float forcedLength = 10f; // Always 10 sec playback

    [Header("Visuals")]
    [SerializeField] private Image panel;
    [SerializeField] private GameObject rawImage;

    [Header("Letters (Keep Original Position)")]
    [SerializeField] private TextMeshProUGUI[] letters; // N, L, U, T, A

    [Header("Studio Name")]
    [SerializeField] private TextMeshProUGUI studioText;

    [Header("Tween Settings")]
    [SerializeField] private SplashTweenType tweenType = SplashTweenType.PuddingBounce;
    [SerializeField] private float perLetterDelay = 0.35f;
    [SerializeField] private float puddingScale = 1.25f;
    [SerializeField] private float appearDuration = 0.65f;

    private bool finished = false;

    void Start()
    {
        PrepareUI();

        player.loopPointReached += OnVideoEnd;
        player.Play();

        // Force video length 10 seconds even if mismatch
        Invoke(nameof(OnVideoEndFallback), forcedLength);
    }

    private void PrepareUI()
    {
        panel.color = Color.black;

        foreach (var letter in letters)
            letter.alpha = 0; // keep position, scale untouched

        studioText.alpha = 0;
    }

    private void OnVideoEndFallback()
    {
        if (!finished)
            OnVideoEnd(null);
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (finished) return;
        finished = true;

        rawImage.SetActive(false);

        // Smooth fade to white
        panel.DOColor(Color.white, 1.2f)
             .SetEase(Ease.InOutQuad)
             .OnComplete(PlaySequence);
    }

    private void PlaySequence()
    {
        Sequence seq = DOTween.Sequence();

        // Letter-by-letter reveal
        foreach (var letter in letters)
        {
            seq.AppendCallback(() => PlayLetterTween(letter));
            seq.AppendInterval(perLetterDelay);
        }

        // Show studio text
        seq.AppendCallback(() => PlayStudioTween(studioText));
        seq.AppendInterval(0.75f);

        seq.OnComplete(() => ServiceLocator
            .Get<SceneScriptManager>()
            .ChangeState(SceneState.Menu));
    }

    private void PlayLetterTween(TMP_Text t)
    {
        t.alpha = 0;

        switch (tweenType)
        {
            case SplashTweenType.PuddingBounce:
                PuddingBounce(t);
                break;

            case SplashTweenType.Elastic:
                ElasticAppear(t);
                break;

            case SplashTweenType.Punch:
                PunchAppear(t);
                break;

            case SplashTweenType.SimpleFade:
                SimpleFade(t);
                break;
        }
    }

    private void PuddingBounce(TMP_Text t)
    {
        Vector3 originalScale = t.transform.localScale;

        t.DOFade(1, 0.1f);
        t.transform.localScale = originalScale * 0.3f;

        Sequence s = DOTween.Sequence()
            .Append(t.transform.DOScale(originalScale * puddingScale, 0.25f)
                .SetEase(Ease.OutBack))
            .Append(t.transform.DOScale(originalScale, 0.2f)
                .SetEase(Ease.OutQuad));
    }

    private void ElasticAppear(TMP_Text t)
    {
        Vector3 originalScale = t.transform.localScale;
        t.transform.localScale = originalScale * 0.4f;

        t.DOFade(1, 0.15f);
        t.transform.DOScale(originalScale, 0.9f)
            .SetEase(Ease.OutElastic, 0.9f, 0.2f);
    }

    private void PunchAppear(TMP_Text t)
    {
        t.DOFade(1, 0.2f);
        t.transform.DOPunchScale(Vector3.one * 0.45f, 0.35f, 8, 0.8f);
    }

    private void SimpleFade(TMP_Text t)
    {
        t.DOFade(1, appearDuration);
    }

    private void PlayStudioTween(TMP_Text t)
    {
        t.alpha = 0;
        t.DOFade(1, 0.35f);

        switch (tweenType)
        {
            case SplashTweenType.PuddingBounce:
            case SplashTweenType.Elastic:
                t.transform.DOScale(1.1f, 0.8f)
                    .From(0.5f)
                    .SetEase(Ease.OutElastic);
                break;

            case SplashTweenType.Punch:
                t.transform.DOPunchScale(Vector3.one * 0.4f, 0.5f, 8, 0.6f);
                break;

            case SplashTweenType.SimpleFade:
                break;
        }
    }
}
