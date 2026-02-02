using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MatchingManager : MonoBehaviour
{
    [Header("Ingame Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject Enemy;

    [Header("External References")]
    public GameObject matchingObjectBox;
    public ItemsLoader itemLoader;
    public FloatingFeedbackText feedbackText;
    public RetroTVPanelEffect effectPanel;

    [Header("UI Buttons")]
    public Button buttonsA;
    public Button buttonsB;
    public Button buttonsX;
    public Button buttonsY;

    [Header("Transition Colors")]
    public Color colorA = Color.white;
    public Color colorB = new Color(0.427f, 0.427f, 0.427f);

    [Header("Settings")]
    public float colorSpeed = 2f;
    public float jumpHeight = 10f;
    public float jumpDuration = 0.8f;

    [Header("Combo Settings")]
    public int comboCount = 0;
    public float comboResetTime = 2f;
    private float lastComboTime;

    private SpriteRenderer matchSprite;
    private Sprite unMatchSprite;
    private readonly List<Button> allButtons = new();
    private List<Sprite> allSprites = new();

    private Sprite correctSprite;
    private bool isLocked;
    private int currentIndex = 0; // Tracks 4-sprite step window
    private int remaining = 0;

    private bool endGame = false;

    private async void Start()
    {
        matchSprite = matchingObjectBox.GetComponentInChildren<SpriteRenderer>();

        if (itemLoader == null)
        {
            itemLoader = GetComponent<ItemsLoader>();
            if (itemLoader == null)
            {
                Debug.LogError("❌ ItemLoader reference not found!");
                return;
            }
        }

        var loadedObjects = await itemLoader.LoadAndCreateMarblesAsync();

        foreach (var obj in loadedObjects)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                allSprites.Add(sr.sprite);
        }

        SetupRound();
        //AnimateButtonsLoop(); // Start jumpy animation
    }

    private void SetupRound()
    {
        if (allSprites.Count == 0)
        {
            Debug.LogWarning("⚠️ Not enough sprites to start!");
            return;
        }

        // Get 4-sprite group window
        remaining = allSprites.Count;

        //List<Sprite> window = allSprites.GetRange(0, 4); // collect 0-4 sprites

        // Pick correct one randomly from this window
        correctSprite = allSprites[0]; //set 0 as correct asume last sprite was removed
        matchSprite.sprite = correctSprite;
        unMatchSprite = allSprites[UnityEngine.Random.Range(1, allSprites.Count)];
        // Build round sprite list
        List<Sprite> roundSprites = new List<Sprite> { correctSprite };
        for (int i = 1; i < 4; i++)
        {
            roundSprites.Add(unMatchSprite);
        }

        ShuffleList(roundSprites); //shuffle for button


        buttonsA.image.sprite = roundSprites[0];

        buttonsB.image.sprite = roundSprites[1];

        buttonsX.image.sprite = roundSprites[2];

        buttonsY.image.sprite = roundSprites[3];

        Debug.Log($"🎯 Round Setup: Correct = {correctSprite.name}, Group {currentIndex / 4 + 1}");
    }

    public void HandleButtonPress(Button button)
    {
        if (endGame) return;
        //isLocked = true;

        Sprite selected = button.image.sprite;

        if (selected == correctSprite)
        {
            Debug.Log($"✅ Correct: {correctSprite.name}");
            //AnimateSuccess(button);
            itemLoader.RemoveFirst();

            // next round setup to select next index loaded for correct sprite

            if (remaining > 1)
            {
                ComboChecker();
                allSprites.RemoveAt(0); // remove used sprite
                SetupRound();
            }
            else
            {
                allSprites.RemoveAt(0); // handle animation item load
                endGame = true;
                buttonsA.onClick.RemoveAllListeners();
                buttonsB.onClick.RemoveAllListeners();
                buttonsX.onClick.RemoveAllListeners();
                buttonsY.onClick.RemoveAllListeners();
                Debug.Log("🎉 All rounds complete!");
            }
        }
        else
        {
            Debug.Log($"❌ Wrong: {selected.name} != {correctSprite.name}");
            AnimateFail(button);
        }
    }

    private void AnimateSuccess(Button button)
    {
        DOTween.Kill(button.transform);
        var img = button.image;
        img.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 10, 1)
            .OnComplete(() =>
            {
                img.color = Color.green;
                DOTween.Sequence()
                    .Append(img.DOFade(0f, 0.3f))
                    .AppendInterval(0.2f)
                    .OnComplete(() =>
                    {
                        img.color = Color.white;
                        img.DOFade(1f, 0.2f);
                        //isLocked = false;
                    });
            });
    }

    private void AnimateFail(Button button)
    {
        DOTween.Kill(button.transform);
        var img = button.image;
        img.transform.DOShakePosition(0.25f, 0.3f);
        img.DOColor(Color.red, 0.1f)
            .OnComplete(() =>
            {
                img.DOColor(Color.white, 0.3f);
                //isLocked = false;
            });
    }

    private void AnimateButtonsLoop()
    {
        foreach (var btn in allButtons)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.DOJumpAnchorPos(rt.anchoredPosition, jumpHeight, 1, jumpDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(UnityEngine.Random.Range(0f, 0.4f)); // Slight offset for variety
        }
    }

    private void Update()
    {
        //float t = Mathf.PingPong(Time.time * colorSpeed, 1f);
        //Color lerpedColor = Color.Lerp(colorA, colorB, t);
        //foreach (var btn in allButtons)
        //    btn.image.color = lerpedColor;

        // Reset combo if idle too long
        if (comboCount > 0 && Time.time - lastComboTime > comboResetTime)
        {
            ResetCombo();
        }

    }

    public void ComboChecker()
    {
        comboCount++;
        lastComboTime = Time.time;

        if (feedbackText != null)
        {
            if (comboCount <= 1)
            {
                feedbackText.Show(LocalizationManager.Instance.Get("CORRECT"), Color.green);
            }
            else
            {
                if (comboCount % 5 == 0)
                {
                    effectPanel.PlayEffect();
                }
                else
                {
                    feedbackText.Show(string.Format(LocalizationManager.Instance.Get("COMBO"), comboCount), Color.green);
                }
            }
        }
    }

    public void ResetCombo()
    {
        comboCount = 0;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
