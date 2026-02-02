using Game.Config;
using Game.Model.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Game.Config.Config;

public class UiController : MonoBehaviour
{
    [Header("For Independent Test Controller")]
    public bool checkedInEditorAsIndependentForTest = true;
    private const string CoreUiSceneName = "CoreInterfaceToUser";
    private SceneFaderEffect SceneFader;
    private GameObject panelDialogue;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI dialogueText;
    private GameObject nextIndicator;
    private GameObject mainButtonUI;
    private GameObject interactButtonUI;
    private Button interactButton;
    private Transform choiceContainer;

    private PlayerEntity playerEntity;

    private void Awake()
    {
        if (SceneFader == null)
        {
            SceneFader = GetComponent<SceneFaderEffect>();
        }

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        RefreshUi();

        // Find UI roots
        var panelObj = GameObject.FindWithTag("TagPanelDialougeAnjing");
        panelDialogue = panelObj;

        var titleObj = GameObject.FindWithTag("TitleText");
        titleText = titleObj != null ? titleObj.GetComponent<TextMeshProUGUI>() : null;

        var dialogueObj = GameObject.FindWithTag("DialogueText");
        dialogueText = dialogueObj != null ? dialogueObj.GetComponent<TextMeshProUGUI>() : null;

        var nextObj = GameObject.FindWithTag("NextDialogue");
        nextIndicator = nextObj;

        var mainBtnObj = GameObject.FindWithTag("MainButton");
        mainButtonUI = mainBtnObj;

        var interactPanelObj = GameObject.FindWithTag("PanelButtonInteraction");
        interactButtonUI = interactPanelObj;

        var interactBtnObj = GameObject.FindWithTag("ButtonInteraction");
        interactButton = interactBtnObj != null ? interactBtnObj.GetComponent<Button>() : null;

        var choiceContainerObj = GameObject.FindWithTag("PanelChoiceContainer");
        choiceContainer = choiceContainerObj != null ? choiceContainerObj.transform : null;

        var playerEntityObj = GameObject.FindWithTag("Player");
        playerEntity = playerEntityObj != null ? playerEntityObj.GetComponent<PlayerEntity>() : null;


        DontDestroyOnLoad(this.gameObject);
    }

    private void OnActiveSceneChanged(Scene previous, Scene current)
    {
        if (Config.isInCutscene)
        {
            RefreshUiAndDoCutScene();
        }
        else
        {
            RefreshUi();
        }

    }

    public void RefreshUi(bool isUseSceneFader = true)
    {
        // If the active scene itself is the core UI scene, do nothing and stop.
        Scene active = SceneManager.GetActiveScene();
        if (active.name == CoreUiSceneName) return;

        // Always target the UiCanvas located in the CoreInterfaceToUser scene (if loaded).
        var uiCanvas = FindUiCanvasInCoreInterfaceScene();
        if (uiCanvas == null) return;

        bool isIntro = SwitchStringToSceneState(active.name) == SceneState.Intro;

        // Enable/disable only the components attached to the UiCanvas (and children).
        ToggleComponentsActive(uiCanvas, !isIntro, isUseSceneFader);
    }

    public void RefreshUiAndDoCutScene()
    {
        // If the active scene itself is the core UI scene, do nothing and stop.
        Scene active = SceneManager.GetActiveScene();
        if (active.name == CoreUiSceneName) return;

        // Always target the UiCanvas located in the CoreInterfaceToUser scene (if loaded).
        var uiCanvas = FindUiCanvasInCoreInterfaceScene();
        if (uiCanvas == null) return;

        // Enable/disable only the components attached to the UiCanvas (and children).
        ToggleComponentsActiveForCutScene(uiCanvas, isInCutscene);
    }

    private void ToggleComponentsActive(GameObject uiCanvas, bool active, bool isUseSceneFader)
    {
        if (uiCanvas == null) return;

        uiCanvas.SetActive(active);

        if (isUseSceneFader)
            SceneFader.RestartFade();

        SetDefaultUiCanvasController();
    }
    private void ToggleComponentsActiveForCutScene(GameObject uiCanvas, bool active)
    {
        if (uiCanvas == null) return;

        uiCanvas.SetActive(active);

        SetForCutSceneUiCanvasController();
        HidePlayer();
    }

    public void HidePlayer()
    {
        playerEntity.gameObject.SetActive(false);
    }
    public void ShowPlayer()
    {
        playerEntity.gameObject.SetActive(true);
    }

    private void SetDefaultUiCanvasController()
    {
        if (interactButtonUI != null) interactButtonUI.SetActive(false);
        if (interactButton != null) interactButton.gameObject.SetActive(false);

        if (panelDialogue != null) panelDialogue.SetActive(false);
        if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);

        if (dialogueText != null) dialogueText.text = "";
        if (titleText != null) titleText.text = "";
        if (mainButtonUI != null) mainButtonUI.SetActive(true);

        GameObject[] joystick = GameObject.FindGameObjectsWithTag("FloatingJoystick");
        foreach (var item in joystick)
        {
            item.gameObject.GetComponent<Image>().enabled = true;
        }

    }

    //Second Safeguard for Cutscene UI
    private void SetForCutSceneUiCanvasController()
    {
        if (interactButtonUI != null) interactButtonUI.SetActive(true);
        if (interactButton != null) interactButton.gameObject.SetActive(true);

        if (panelDialogue != null) panelDialogue.SetActive(true);
        if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);

        if (mainButtonUI != null) mainButtonUI.gameObject.SetActive(false);

        //mainButtonPanel.gameObject.GetComponent<Renderer>().enabled = true;
        //statusUiPanel.gameObject.GetComponent<Renderer>().enabled = true;
        GameObject[] joystick = GameObject.FindGameObjectsWithTag("FloatingJoystick");
        foreach (var item in joystick)
        {
            item.gameObject.GetComponent<Image>().enabled = false;
        }


    }

    private GameObject FindUiCanvasInCoreInterfaceScene()
    {
        // Iterate through loaded scenes and find the one named CoreInterfaceToUser.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!checkedInEditorAsIndependentForTest)
            {
                if (!scene.isLoaded) continue;
                if (scene.name != CoreUiSceneName) continue;
            }

            // Search root game objects for the UiCanvas tag.
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.CompareTag("UiCanvas"))
                    return root;

                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.CompareTag("UiCanvas"))
                        return child.gameObject;
                }
            }
        }

        return null;
    }
}
