using Game.Config;
using Game.Model.Player;
using UnityEngine;
using UnityEngine.Playables;

public class CutSceneOrchestrator : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector playableDirector;
    private GameObject scriptSignal;

    private UiController uiController;
    private PlayerEntity playerEntity;

    private bool isCutscenePlaying;
    [SerializeField] private DialogueCutSceneController componentDialog;

    private void Awake()
    {
        playerEntity = FinderTagHelper
            .FindPlayer<PlayerEntity>()
            ?.GetComponent<PlayerEntity>();

        var uiCanvasSupport = FinderTagHelper.FindTagged("UiCanvasSupport");
        uiController = uiCanvasSupport.GetComponentInChildren<UiController>();

        if (playableDirector != null)
        {
            playableDirector.played += OnTimelinePlayed;
            playableDirector.stopped += OnTimelineStopped;
            playableDirector.paused += OnTimelinePaused;
        }
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
        {
            playableDirector.played -= OnTimelinePlayed;
            playableDirector.stopped -= OnTimelineStopped;
            playableDirector.paused -= OnTimelinePaused;
        }
    }

    // =====================================================
    // 🎬 Timeline Events
    // =====================================================

    private void OnTimelinePlayed(PlayableDirector director)
    {
        SetCutsceneState(true);
    }

    private void OnTimelinePaused(PlayableDirector director)
    {
        // Optional: treat pause as still in cutscene
        SetCutsceneState(true);
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        SetCutsceneState(false);
        director.gameObject.SetActive(false);
        uiController.ShowPlayer();
        uiController.RefreshUi(false);
    }

    // =====================================================
    // 🧠 Centralized State Control
    // =====================================================

    private void SetCutsceneState(bool active)
    {
        //if (componentDialog.currentIndex >= (componentDialog.currentData.lines?.Length ?? 0) && componentDialog.canDoInteraction)
        //{
        //    OnTimelineStopped(playableDirector);
        //    return;
        //}

        if (isCutscenePlaying == active)
            return; // no redundant calls

        isCutscenePlaying = active;
        Config.isInCutscene = active;

        uiController.RefreshUiAndDoCutScene();

        // Optional but recommended
        //if (playerEntity != null)
        //{
        //    playerEntity.SetInputEnabled(!active);
        //}
    }
}
