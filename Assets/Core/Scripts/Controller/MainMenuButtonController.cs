using System.Collections;
using UnityEngine;
using static Game.Config.Config;

public class MainMenuButtonController : MonoBehaviour
{
    [Header("Assign in Inspector (preferred)")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private ConsentPopupUI consentPopupUI;
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject mainPanelButton;
    private GameEventBus _eventBus;
    private StargateReverseBurstSpawner_Extreme stargateSpawner;
    [SerializeField] private AudioSource audioToFadeOut;

    private void Awake()
    {
        _eventBus = GameEventBus.Instance;
        stargateSpawner = GetComponent<StargateReverseBurstSpawner_Extreme>();
        // Search in root canvas, even if inactive
        var root = transform.root;

        if (settingsPanel == null)
            settingsPanel = FindDeepChildIncludingInactive(root, "SettingsPanel")?.gameObject;

        Debug.Log($"[MainMenuButtonController] Awake -> creditsPanel: {settingsPanel?.name ?? "null"}");
    }

    public void PlayGameButton()
    {
        StartCoroutine(HandlePlayGame());

    }

    private IEnumerator HandlePlayGame()
    {
        title.SetActive(false);
        background.SetActive(false);
        mainPanelButton.SetActive(false);
        stargateSpawner.enabled = false;
        stargateSpawner.ClearAndStop();
        consentPopupUI.Show();
        audioToFadeOut.Stop();
        yield return new WaitUntil(() => consentPopupUI.gameObject.activeSelf == false);
        ServiceLocator.Get<SceneScriptManager>().ChangeState(SceneState.CoreLoader);
    }

    public void OptionButton()
    {
        if (settingsPanel == null) { Debug.LogWarning("SettingsPanel is null"); return; }
        settingsPanel.SetActive(true);
    }

    public void CloseButton()
    {
        if (settingsPanel == null) { Debug.LogWarning("SettingsPanel is null"); return; }
        settingsPanel.SetActive(false);
    }

    public void ExtraButton()
    {
        ServiceLocator.Get<SceneScriptManager>().ChangeState(SceneState.Credits);
    }

    private static Transform FindDeepChildIncludingInactive(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }
        return null;
    }
}
