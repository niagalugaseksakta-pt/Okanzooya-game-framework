using Assets.CoreFramework.Scripts.Controller;
using Assets.CoreFramework.Scripts.Managers.Dialouge.Core;
using Game.Model.Player;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


//Npc Object
[RequireComponent(typeof(Collider2D))]
public class DialogueControllerIngame : MonoBehaviour
{

    // =========================================================
    // Main Character
    // =========================================================
    [SerializeField] private PlayerEntity playerEntity;
    [SerializeField] private ShopController shopController;
    private Animator npcAnimator;
    // =========================================================
    // UI
    // =========================================================
    // Typing
    [Header("Typing Effect")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private string fullLineText;

    [Header("Dialogue UI")]
    protected GameObject panelDialogue; // PanelDialogue (GameObject)
    protected TextMeshProUGUI titleText; // TitleText (TMP)
    protected TextMeshProUGUI dialogueText; // DialogueText (TMP)
    protected GameObject nextIndicator; // NextDialogue

    [Header("Interaction UI")]
    protected GameObject mainButtonUI; // MainButton
    protected GameObject interactButtonUI; // PanelButtonInteraction
    protected Button interactButton; // ButtonInteraction
    protected Button interactExitButton; // ButtonExitInteraction

    [Header("Choices")]
    protected Transform choiceContainer; // PanelChoicesContainer (Transform parent)
    [SerializeField] private GameObject choiceButtonPrefab; // prefab must be assigned in inspector

    // =========================================================
    // SETTINGS
    // =========================================================
    [Header("Settings")]
    public string dialogueFilename;
    public string currentDialogueFilename;
    // =========================================================
    // INPUT
    // =========================================================
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;

    // =========================================================
    // VOICE
    // =========================================================
    [Header("Voice")]
    public DialogueVoice[] voices;
    private AudioSource audioSource;


    // =========================================================
    // STATE
    // =========================================================
    private DialogueData currentData;
    private int currentIndex;
    private bool playerInRange;
    private bool dialoguePrepared;
    private bool dialogueFinished;


    // =========================================================
    // UNITY
    // =========================================================
    private void Awake()
    {

        // =========================
        // FIND UI roots (robust across scenes and inactive objects)
        // =========================
        var panelObj = FinderTagHelper.FindTagged("TagPanelDialougeAnjing");
        panelDialogue = panelObj;

        var titleObj = FinderTagHelper.FindTagged("TitleText");
        titleText = titleObj != null ? titleObj.GetComponent<TextMeshProUGUI>() : null;

        var dialogueObj = FinderTagHelper.FindTagged("DialogueText");
        dialogueText = dialogueObj != null ? dialogueObj.GetComponent<TextMeshProUGUI>() : null;

        var nextObj = FinderTagHelper.FindTagged("NextDialogue");
        nextIndicator = nextObj;

        var mainBtnObj = FinderTagHelper.FindTagged("MainButton");
        mainButtonUI = mainBtnObj;

        var interactPanelObj = FinderTagHelper.FindTagged("PanelButtonInteraction");
        interactButtonUI = interactPanelObj;

        var interactBtnObj = FinderTagHelper.FindTagged("ButtonInteraction");
        interactButton = interactBtnObj != null ? interactBtnObj.GetComponent<Button>() : null;

        var interactBtnExitObj = FinderTagHelper.FindTagged("ButtonExitInteraction");
        interactExitButton = interactBtnExitObj != null ? interactBtnExitObj.GetComponent<Button>() : null;

        var choiceContainerObj = FinderTagHelper.FindTagged("PanelChoiceContainer");
        choiceContainer = choiceContainerObj != null ? choiceContainerObj.transform : null;

        // =========================
        // FIND PLAYER SAFELY
        // =========================
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }

        // =========================
        // COMPONENTS
        // =========================
        npcAnimator = GetComponentInChildren<Animator>(true);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // =========================
        // UI SAFETY
        // =========================
        if (panelDialogue != null) panelDialogue.SetActive(false);
        if (interactButtonUI != null) interactButtonUI.SetActive(false);

        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(AdvanceDialogue);
        }
        else
        {
            Debug.LogWarning("[DialogueController] Interact Button is not assigned or missing Button component.");
        }

        if (interactExitButton != null)
        {
            interactExitButton.gameObject.SetActive(false);
            interactExitButton.onClick.RemoveAllListeners();
            interactExitButton.onClick.AddListener(ExitAdvanceDialogue);
        }
        else
        {
            Debug.LogWarning("[DialogueController] Interact Exit Button is not assigned or missing Button component.");
        }

        if (titleText == null)
            Debug.LogWarning("[DialogueController] TitleText (TextMeshProUGUI) not found.");
        if (dialogueText == null)
            Debug.LogWarning("[DialogueController] DialogueText (TextMeshProUGUI) not found.");
        if (choiceButtonPrefab == null)
            Debug.LogWarning("[DialogueController] Choice Button Prefab is not assigned.");
        if (choiceContainer == null)
            Debug.LogWarning("[DialogueController] PanelChoicesContainer not found.");
    }


    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractInput;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractInput;
            interactAction.action.Disable();
        }
    }

    // =========================================================
    // TRIGGER ZONE
    // =========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        currentDialogueFilename = dialogueFilename;
        PrepareDialogue();

        if (mainButtonUI != null) mainButtonUI.SetActive(false);
        if (interactButtonUI != null) interactButtonUI.SetActive(true);
        if (interactButton != null) interactButton.gameObject.SetActive(true);
        if (interactExitButton != null) interactExitButton.gameObject.SetActive(true);
        if (npcAnimator != null) npcAnimator.Play("Talk");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        CleanupAll();
    }

    // =========================================================
    // PREPARE
    // =========================================================
    private void PrepareDialogue()
    {

        currentData = DialogueLoader.LoadDialogue(currentDialogueFilename);
        currentIndex = 0;

        dialoguePrepared = true;
        dialogueFinished = false;

        if (titleText != null)
            titleText.text = currentData != null ? currentData.title : "";
        if (dialogueText != null)
            dialogueText.text = "";

        if (panelDialogue != null) panelDialogue.SetActive(false);
        if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);
        if (nextIndicator != null) nextIndicator.SetActive(false);
    }

    // =========================================================
    // INTERACTION
    // =========================================================
    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        if (!playerInRange)
            return;

        AdvanceDialogue();
    }

    private void AdvanceDialogue()
    {
        if (playerEntity != null)
            playerEntity.isInInteractions = true;

        if (!dialoguePrepared || dialogueFinished || currentData == null)
            return;

        if (panelDialogue != null && !panelDialogue.activeSelf)
            panelDialogue.SetActive(true);

        // ⏭️ If typing, finish instantly
        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        if (currentIndex >= (currentData.lines?.Length ?? 0))
        {
            EndDialogue();
            return;
        }

        ShowLine(currentIndex);
    }
    private void ExitAdvanceDialogue()
    {
        EndDialogue();
        return;
    }



    // =========================================================
    // DISPLAY
    // =========================================================
    private void ShowLine(int index)
    {
        ClearChoices();

        var line = currentData.lines[index];
        fullLineText = line.dialogueText;
        if (dialogueText != null)
            dialogueText.text = "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line));

        AudioClip voice = GetVoice(line.voiceKey);
        if (voice != null)
            audioSource.PlayOneShot(voice, 0.8f);
    }

    private IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
        if (nextIndicator != null) nextIndicator.SetActive(false);

        foreach (char c in fullLineText)
        {
            if (dialogueText != null)
                dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        // === AFTER TYPING FINISHED ===
        if (line.choices != null && line.choices.Length > 0)
        {
            ShowChoices(line.choices);
        }
        else
        {
            if (nextIndicator != null) nextIndicator.SetActive(true);
            currentIndex++;
        }
    }

    private void CompleteTyping()
    {
        if (!isTyping)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (dialogueText != null)
            dialogueText.text = fullLineText;
        isTyping = false;
        typingCoroutine = null;

        var line = currentData.lines[currentIndex];

        if (line.choices != null && line.choices.Length > 0)
        {
            ShowChoices(line.choices);
        }
        else
        {
            if (nextIndicator != null) nextIndicator.SetActive(true);
            currentIndex++;
        }
    }



    private void ShowChoices(DialogueChoice[] choices)
    {
        if (choiceContainer == null)
        {
            Debug.LogWarning("[DialogueController] Cannot show choices: choiceContainer missing.");
            return;
        }

        if (choiceButtonPrefab == null)
        {
            Debug.LogWarning("[DialogueController] Cannot show choices: choiceButtonPrefab missing.");
            return;
        }

        choiceContainer.gameObject.SetActive(true);

        foreach (var choice in choices)
        {
            var btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (txt != null)
                txt.text = choice.choiceText;

            if (btn != null)
                btn.onClick.AddListener(() => OnChoiceSelected(choice));
            else
                Debug.LogWarning("[DialogueController] Choice button prefab is missing a Button component.");
        }
    }


    private void OnChoiceSelected(DialogueChoice choice)
    {
        ClearChoices();

        switch (choice.actionType)
        {
            case "open_shop":
                if (shopController != null)
                    shopController.LoadSceneShop();
                else
                    Debug.LogWarning("[DialogueController] shopController not assigned.");
                break;

            case "open_quest":
                //OpenQuestUI();
                break;

            case "next_dialogue":
                LoadNextDialogue(choice.nextFile);
                break;

            default:
                Debug.LogWarning($"Unknown actionType: {choice.actionType}");
                EndDialogue();
                break;
        }
    }

    private void LoadNextDialogue(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            Debug.LogError("next_dialogue requires nextFile");
            EndDialogue();
            return;
        }

        currentDialogueFilename = filename;
        //dialogueFilename = filename;
        PrepareDialogue();
        AdvanceDialogue();
    }

    private void ClearChoices()
    {
        if (choiceContainer == null)
            return;

        foreach (Transform child in choiceContainer)
        {
            if (child != null && child.gameObject != null)
                Destroy(child.gameObject);
        }

        choiceContainer.gameObject.SetActive(false);
    }


    private AudioClip GetVoice(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        if (voices == null)
            return null;

        foreach (var v in voices)
            if (v.key == key)
                return v.clip;

        return null;
    }

    // =========================================================
    // END
    // =========================================================
    private void EndDialogue()
    {
        dialogueFinished = true;
        if (nextIndicator != null) nextIndicator.SetActive(false);

        if (mainButtonUI != null) mainButtonUI.SetActive(true);
        if (playerEntity != null) playerEntity.isInInteractions = true;
        CleanupAll();
    }

    // =========================================================
    // CLEANUP
    // =========================================================
    private void CleanupAll()
    {
        playerInRange = false;
        dialoguePrepared = false;
        dialogueFinished = false;

        if (interactButtonUI != null) interactButtonUI.SetActive(false);
        if (interactButton != null) interactButton.gameObject.SetActive(false);

        if (panelDialogue != null) panelDialogue.SetActive(false);
        if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);

        currentDialogueFilename = dialogueFilename;
        if (dialogueText != null) dialogueText.text = "";
        if (titleText != null) titleText.text = "";
        if (mainButtonUI != null) mainButtonUI.SetActive(true);
        if (playerEntity != null) playerEntity.isInInteractions = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        fullLineText = "";
        if (npcAnimator != null) npcAnimator.Play("Idle");
    }
}
