using Game.Model.Player;
using UnityEngine;
using static Game.Config.Config;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class CheckPointSpawn : MonoBehaviour
{

    [Header("Checkpoint Settings")]
    public bool isActivated = false;

    public bool restrictTeam = false;
    public int allowedTeamID = 1;

    [Header("Optional FX")]
    public GameObject activateEffect;

    [Header("Save")]
    [SerializeField] private string checkpointID;

    private SceneScriptManager _sceneManager;

    private void Awake()
    {
        ServiceLocator.ReInit();

        _sceneManager = ServiceLocator.Get<SceneScriptManager>();
        if (_sceneManager == null)
        {
            Debug.LogError("[NextStageController] SceneScriptManager tidak ditemukan dari ServiceLocator.");
        }
        SaveManager.Init();
        DontDestroyOnLoad(gameObject);
    }

    private void Reset()
    {
        SaveManager.Init();
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (string.IsNullOrEmpty(checkpointID))
            checkpointID = gameObject.name;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerEntity entity = other.GetComponent<PlayerEntity>();
        if (entity == null) return;

        if (restrictTeam && entity.TeamID != allowedTeamID)
            return;

        ActivateCheckpoint(entity);
    }

    private void ActivateCheckpoint(PlayerEntity entity)
    {
        if (isActivated) return;

        isActivated = true;

        SaveCheckpointToStatBlock(entity);

        Debug.Log($"[Checkpoint] Activated: {checkpointID}");
    }

    private void SaveCheckpointToStatBlock(PlayerEntity entity)
    {
        SceneState current = _sceneManager._currentState;

        if (entity.Stats == null)
        {
            Debug.LogError("[Checkpoint] StatBlock tidak ditemukan.");
            return;
        }

        entity.Stats.checkpointID = checkpointID;
        entity.Stats.lastCheckpointPosition = transform.position;
        entity.Stats.lastScene = current;
        entity.Stats.isFreshStart = false;

        try
        {
            SaveManager._instance.Save(entity);
        }
        catch
        {
            Debug.LogWarning("[Checkpoint] SaveManager belum di-assign.");
        }
    }
}
