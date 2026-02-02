using Game.Model.Player;
using UnityEngine;
using static Game.Config.Config;

public class SceneExecuteTrigger : MonoBehaviour
{
    private SceneScriptManager _sceneManager;
    private SceneState current;
    private void Awake()
    {
        ServiceLocator.ReInit();

        _sceneManager = ServiceLocator.Get<SceneScriptManager>();
        if (_sceneManager == null)
        {
            Debug.LogError("[NextStageController] SceneScriptManager tidak ditemukan dari ServiceLocator.");
        }
        SaveManager.Init();

        current = _sceneManager._currentState;
        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var ctx = NextStageController.PreparedContext;

        if (ctx == null)
        {
            Debug.LogWarning("[ExecuteTrigger] No prepared context.");
            return;
        }


        PlayerEntity entity = col.GetComponent<PlayerEntity>();

        if (entity.Stats == null)
        {
            Debug.LogError("[Checkpoint] StatBlock tidak ditemukan.");
            return;
        }

        entity.Stats.lastScene = GetComponentInChildren<NextStageController>().targetScene;
        entity.Stats.lastPortalNamebyScene = GetComponent<PortalEntity>().portalNamebyScene;
        entity.Stats.nextPortal = GetComponent<PortalEntity>().nextPortalId;
        entity.Stats.isThroughDoor = true;

        try
        {
            SaveManager._instance.Save(entity);
        }
        catch
        {
            Debug.LogWarning("[Checkpoint] SaveManager belum di-assign.");
        }

        _sceneManager.ChangeState(ctx.targetState);
        NextStageController.PreparedContext = null;
    }

}
