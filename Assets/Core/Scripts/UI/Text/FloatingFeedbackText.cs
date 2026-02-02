using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingFeedbackText : MonoBehaviour
{
    [Header("🎨 Visuals")]
    public TMP_Text textElement;
    public Color highlightColor = Color.yellow;
    public float floatDistance = 80f;
    public float scaleIn = 1.2f;
    public float duration = 1.5f;
    public bool autoDestroy = false;

    private Sequence seq;

    void Awake()
    {
        if (!textElement) textElement = GetComponent<TMP_Text>();
        textElement.alpha = 0;
    }

    public void Show(string message, Color color)
    {
        Shake();
        textElement.text = message;
        textElement.color = color;
        textElement.alpha = 0;
        transform.localScale = Vector3.one * 0.4f;
        gameObject.SetActive(true);
        if (seq != null && seq.IsActive())
            seq.Kill();

        seq = DOTween.Sequence();

        // ⚡ Animate
        seq.Append(textElement.DOFade(1f, 0.2f));
        seq.Join(transform.DOScale(scaleIn, 0.4f).SetEase(Ease.OutBack));
        seq.Join(transform.DOLocalMoveY(transform.localPosition.y + floatDistance, duration).SetEase(Ease.OutCubic));
        seq.AppendInterval(0.3f);
        seq.Append(textElement.DOFade(0f, 0.4f));
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void Shake()
    {
        transform.DOShakePosition(0.4f, 10f, 10, 90, false, true);
    }
}
