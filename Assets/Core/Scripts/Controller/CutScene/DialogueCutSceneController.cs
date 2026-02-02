using Assets.CoreFramework.Scripts.Managers.Dialouge.Core;
using Game.Config;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueCutSceneController : MonoBehaviour
{

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
    public DialogueData currentData;
    public int currentIndex;
    private bool dialoguePrepared;
    private bool dialogueFinished;
    internal bool canDoInteraction;


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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // =========================
        // UI SAFETY
        // =========================
        if (panelDialogue != null) panelDialogue.SetActive(true);
        if (interactButtonUI != null) interactButtonUI.SetActive(false);

        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(AdvanceDialogueCutScene);
        }
        else
        {
            Debug.LogWarning("[DialogueController] Interact Button is not assigned or missing Button component.");
        }

        if (interactExitButton != null)
        {
            interactExitButton.gameObject.SetActive(false);
            interactExitButton.onClick.RemoveAllListeners();
            interactExitButton.onClick.AddListener(ExitAdvanceDialogueCutScene);
        }
        else
        {
            Debug.LogWarning("[DialogueController] Interact Exit Button is not assigned or missing Button component.");
        }

        if (titleText == null)
            Debug.LogWarning("[DialogueController] TitleText (TextMeshProUGUI) not found.");
        if (dialogueText == null)
            Debug.LogWarning("[DialogueController] DialogueText (TextMeshProUGUI) not found.");

    }


    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractInput;
        }

        currentDialogueFilename = dialogueFilename;
        PrepareDialogueCutScene();


    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractInput;
            interactAction.action.Disable();
        }

        CleanupAllCutScene();
    }

    // =========================================================
    // PREPARE
    // =========================================================
    private void PrepareDialogueCutScene()
    {

        currentData = DialogueLoader.LoadDialogue(currentDialogueFilename);
        currentIndex = 0;

        dialoguePrepared = true;
        dialogueFinished = false;

        if (titleText != null)
            titleText.text = currentData != null ? currentData.title : "";
        if (dialogueText != null)
            dialogueText.text = "";

        if (panelDialogue != null) panelDialogue.SetActive(true);
        if (nextIndicator != null) nextIndicator.SetActive(false);
        if (mainButtonUI != null) mainButtonUI.SetActive(false);
        if (interactButtonUI != null) interactButtonUI.SetActive(true);
        if (interactButton != null) interactButton.gameObject.SetActive(true);
        if (interactExitButton != null) interactExitButton.gameObject.SetActive(false);
        GameObject[] joystick = GameObject.FindGameObjectsWithTag("FloatingJoystick");
        foreach (var item in joystick)
        {
            item.gameObject.GetComponent<Image>().enabled = false;
        }
    }

    // =========================================================
    // INTERACTION
    // =========================================================
    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        AdvanceDialogueCutScene();
    }

    public void AdvanceDialogueCutScene()
    {

        if (!dialoguePrepared || dialogueFinished || currentData == null)
            return;

        if (panelDialogue != null && !panelDialogue.activeSelf)
            panelDialogue.SetActive(true);

        // ⏭️ If typing, finish instantly
        if (isTyping)
        {
            CompleteTypingCutScene();
            return;
        }

        if (currentIndex >= (currentData.lines?.Length ?? 0))
        {
            EndDialogueCutScene();
            return;
        }

        ShowLineCutScene(currentIndex);
    }
    private void ExitAdvanceDialogueCutScene()
    {
        EndDialogueCutScene();
        return;
    }



    // =========================================================
    // DISPLAY
    // =========================================================
    private void ShowLineCutScene(int index)
    {
        var line = currentData.lines[index];
        fullLineText = line.dialogueText;
        if (dialogueText != null)
            dialogueText.text = "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLineCutScene(line));

        AudioClip voice = GetVoiceCutScene(line.voiceKey);
        if (voice != null)
            audioSource.PlayOneShot(voice, 0.8f);
    }

    private IEnumerator TypeLineCutScene(DialogueLine line)
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


        if (nextIndicator != null) nextIndicator.SetActive(true);
        currentIndex++;

        yield return new WaitForSeconds(Config.DIALOG_WAIT_TIME);
        AdvanceDialogueCutScene();

    }

    private void CompleteTypingCutScene()
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

        if (nextIndicator != null) nextIndicator.SetActive(true);
        currentIndex++;

    }

    private void LoadNextDialogueCutScene(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            Debug.LogError("next_dialogue requires nextFile");
            EndDialogueCutScene();
            return;
        }

        currentDialogueFilename = filename;
        //dialogueFilename = filename;
        PrepareDialogueCutScene();
        AdvanceDialogueCutScene();
    }

    private AudioClip GetVoiceCutScene(string key)
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
    public void EndDialogueCutScene()
    {
        dialogueFinished = true;
        if (nextIndicator != null) nextIndicator.SetActive(false);

        if (mainButtonUI != null) mainButtonUI.SetActive(true);
        CleanupAllCutScene();
    }

    // =========================================================
    // CLEANUP
    // =========================================================
    private void CleanupAllCutScene()
    {
        dialoguePrepared = false;
        dialogueFinished = false;

        if (interactButtonUI != null) interactButtonUI.SetActive(false);
        if (interactButton != null) interactButton.gameObject.SetActive(false);

        if (panelDialogue != null) panelDialogue.SetActive(false);

        currentDialogueFilename = dialogueFilename;
        if (dialogueText != null) dialogueText.text = "";
        if (titleText != null) titleText.text = "";
        if (mainButtonUI != null) mainButtonUI.SetActive(true);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        fullLineText = "";
    }
}
