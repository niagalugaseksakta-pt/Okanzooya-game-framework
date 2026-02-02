using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleMovieCreditScroller : MonoBehaviour
{
    [Header("=== CREDIT LINES ===")]
    public CreditLine[] credits;

    [Header("=== UI ===")]
    public RectTransform contentRoot;
    public TextMeshProUGUI linePrefab;

    [Header("=== END LOGO ===")]
    public Image endLogo;
    public float logoDelay = 1.5f;

    [Header("=== MOTION ===")]
    public float scrollSpeed = 50f;
    public float startY = -600f;
    public float endY = 1400f;
    public float lineSpacing = 42f;

    private bool logoShown;

    void Start()
    {
        BuildCredits();
        contentRoot.anchoredPosition = new Vector2(0, startY);
        endLogo.gameObject.SetActive(false);
    }

    void Update()
    {
        contentRoot.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (!logoShown && contentRoot.anchoredPosition.y > endY)
        {
            logoShown = true;
            Invoke(nameof(ShowLogo), logoDelay);
        }
    }

    void BuildCredits()
    {
        float y = 0f;

        foreach (var credit in credits)
        {
            var line = Instantiate(linePrefab, contentRoot);
            line.text = FormatLine(credit);
            line.rectTransform.anchoredPosition = new Vector2(0, y);
            y -= lineSpacing;
        }
    }

    string FormatLine(CreditLine credit)
    {
        return $"{credit.role} — {credit.content}";
    }

    void ShowLogo()
    {
        endLogo.gameObject.SetActive(true);
    }
}
