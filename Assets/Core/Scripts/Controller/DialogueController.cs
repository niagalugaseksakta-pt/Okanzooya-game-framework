using Assets.CoreFramework.Scripts.Managers.Dialouge.Core;
using DG.Tweening; // 🔸 Requires DOTween
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitLeft;
    public Image portraitRight;
    public Image backgroundScene;
    public GameObject nextIndicator;
    public GameObject choiceContainer;
    public GameObject choiceButtonPrefab;

    [Header("Settings")]
    public float typingSpeed = 0.03f;
    public float clickCooldown = 0.25f;

    private DialogueData currentDialogue;
    private int currentIndex;
    private bool isTyping;
    private bool canContinue;
    private Coroutine timerCoroutine;
    private float lastClickTime;

    // 🔹 Optional: Speaker-to-portrait mapping
    [Header("Speaker Settings")]
    public Dictionary<string, string> speakerPortraitMap = new Dictionary<string, string>();

    private void Awake()
    {
        // Example mapping (or load from JSON)
        speakerPortraitMap["Hero"] = "Hero/neutral";
        speakerPortraitMap["Villain"] = "Villain/angry";
        speakerPortraitMap["Guide"] = "Guide/smile";
    }

    public void StartDialogueFromFile(string fileName)
    {
        currentDialogue = DialogueLoader.LoadDialogue(fileName);
        if (currentDialogue == null)
        {
            Debug.LogError($"Failed to load dialogue: {fileName}");
            return;
        }

        currentIndex = 0;
        gameObject.SetActive(true);
        choiceContainer.SetActive(false);
        StartCoroutine(DisplayLine());
    }

    private IEnumerator DisplayLine()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        isTyping = true;
        canContinue = false;
        nextIndicator.SetActive(false);
        choiceContainer.SetActive(false);

        var line = currentDialogue.lines[currentIndex];
        nameText.text = line.speakerName;
        dialogueText.text = "";

        // 🔹 Load background
        if (!string.IsNullOrEmpty(line.scene))
            LoadSceneBackground(line.scene);

        // 🔹 Load portrait based on speaker or path
        string portraitKey = !string.IsNullOrEmpty(line.portraitPath)
            ? line.portraitPath
            : (speakerPortraitMap.ContainsKey(line.speakerName)
                ? speakerPortraitMap[line.speakerName]
                : null);

        if (portraitKey != null)
            LoadPortrait(portraitKey, line.portraitSide);

        // 🔹 Highlight active speaker
        HighlightSpeaker(line.portraitSide);

        // 🔹 Animate portrait if emotion is defined
        if (!string.IsNullOrEmpty(line.emotion))
            AnimateEmotion(line.portraitSide, line.emotion);

        // 🔹 Typing effect
        foreach (char c in line.dialogueText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (line.choices != null && line.choices.Length > 0)
            ShowChoices(line.choices);
        else
        {
            canContinue = true;
            nextIndicator.SetActive(true);

            float timer = line.timer > 0 ? line.timer :
                (currentDialogue.autoTimer > 0 ? currentDialogue.autoTimer : 0);

            if (timer > 0)
                timerCoroutine = StartCoroutine(AutoContinueAfterDelay(timer));
        }
    }

    private void HighlightSpeaker(string side)
    {
        float dimAlpha = 0.35f;
        float brightAlpha = 1f;

        if (side == "left")
        {
            portraitLeft.DOFade(brightAlpha, 0.3f);
            portraitRight.DOFade(dimAlpha, 0.3f);
        }
        else
        {
            portraitRight.DOFade(brightAlpha, 0.3f);
            portraitLeft.DOFade(dimAlpha, 0.3f);
        }
    }

    private void AnimateEmotion(string side, string emotion)
    {
        Image target = (side == "left") ? portraitLeft : portraitRight;
        switch (emotion.ToLower())
        {
            case "shake":
                target.transform.DOShakePosition(0.5f, 5f, 15, 45);
                break;
            case "bounce":
                target.transform.DOPunchScale(Vector3.one * 0.1f, 0.4f, 10);
                break;
            case "fadein":
                target.DOFade(1f, 0.5f).From(0f);
                break;
            case "fadeout":
                target.DOFade(0f, 0.5f);
                break;
            default:
                break;
        }
    }

    private void LoadPortrait(string path, string side)
    {
        if (string.IsNullOrEmpty(side))
        {
            Debug.LogWarning("Portrait side not specified.");
            return;
        }

        // Determine target
        Image target = side.Equals("left", System.StringComparison.OrdinalIgnoreCase)
            ? portraitLeft
            : portraitRight;

        if (target == null)
        {
            Debug.LogWarning($"Portrait target not assigned for side '{side}'.");
            return;
        }

        // Try to build path
        string fullPath = string.IsNullOrEmpty(path)
            ? string.Empty
            : Path.Combine(Application.streamingAssetsPath, "Dialogues", path);

        if (!string.IsNullOrEmpty(fullPath) && !Path.HasExtension(fullPath))
            fullPath += ".png";

        Sprite loadedSprite = null;

        // Try loading file
        if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
        {
            try
            {
                byte[] data = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(data))
                {
                    loadedSprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f)
                    );
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error loading portrait '{fullPath}': {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Portrait not found: {fullPath}");
        }

        // 🧩 Fallback: use Unity's default sprite or button sprite
        if (loadedSprite == null)
        {
            // Option 1: use built-in "UISprite" from Default UI Material
            loadedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

            // Option 2: fallback from an existing UI element
            if (loadedSprite == null)
            {
                Button fallbackButton = target.GetComponentInParent<Button>();
                if (fallbackButton != null)
                    loadedSprite = fallbackButton.targetGraphic is Image img ? img.sprite : null;
            }

            if (loadedSprite == null)
            {
                Debug.LogWarning("No default system sprite found — target image will remain empty.");
            }
        }

        // Apply
        target.sprite = loadedSprite;
        target.color = Color.white;
    }


    private void LoadSceneBackground(string sceneName)
    {
        string fullPath = $"{Application.streamingAssetsPath}/Scenes/{sceneName}.png";
        if (File.Exists(fullPath))
        {
            byte[] imageData = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageData);
            backgroundScene.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning($"Scene background not found: {fullPath}");
        }
    }

    private IEnumerator AutoContinueAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (canContinue)
            NextLine();
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Time.time - lastClickTime < clickCooldown)
            return; // Prevent double clicks

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                lastClickTime = Time.time;
                HandleDialogueClick();
            }
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            lastClickTime = Time.time;
            HandleDialogueClick();
        }
#endif
    }

    private void HandleDialogueClick()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentDialogue.lines[currentIndex].dialogueText;
            isTyping = false;
            canContinue = true;
            nextIndicator.SetActive(true);
        }
        else if (canContinue)
        {
            NextLine();
        }
    }

    private void NextLine()
    {
        currentIndex++;
        if (currentIndex < currentDialogue.lines.Length)
            StartCoroutine(DisplayLine());
        else
            EndDialogue();
    }

    private void ShowChoices(DialogueChoice[] choices)
    {
        choiceContainer.SetActive(true);
        nextIndicator.SetActive(false);

        foreach (Transform child in choiceContainer.transform)
            Destroy(child.gameObject);

        foreach (var choice in choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choiceContainer.transform);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(choice.nextFile))
                    StartDialogueFromFile(choice.nextFile);
                else
                    EndDialogue();
            });
        }
    }

    private void EndDialogue()
    {
        Debug.Log($"Dialogue {currentDialogue.title} ended.");
        gameObject.SetActive(false);
    }
}
