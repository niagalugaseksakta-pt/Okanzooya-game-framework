using UnityEngine;
using static Game.Config.Config;

public class NextStageController : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Biarkan kosong untuk otomatis ke next scene berdasarkan urutan enum.")]
    public SceneState targetScene = SceneState.None;

    private SceneScriptManager _sceneManager;

    public bool preloadAdditive = true;

    public static SceneTransitionContext PreparedContext;

    private void Awake()
    {
        ServiceLocator.ReInit();

        _sceneManager = ServiceLocator.Get<SceneScriptManager>();
        if (_sceneManager == null)
        {
            Debug.LogError("[NextStageController] SceneScriptManager tidak ditemukan dari ServiceLocator.");
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Deprecated
        //SceneState next = ResolveSelectedState();
        //_sceneManager.ChangeState(next);


        SceneState next = ResolveSelectedState();
        PreparedContext = new SceneTransitionContext
        {
            targetState = next,
            useAdditive = preloadAdditive,
            preloadOnly = true
        };

        Debug.Log($"[PrepareTrigger] Prepared {next}");
    }

    //Handle go to next stage for button click or other event
    public void GoToSelectedStage()
    {
        SceneState next = ResolveSelectedState();
        _sceneManager.ChangeState(next);
    }

    /// <summary>
    /// Menentukan scene berikutnya berdasarkan parameter atau fallback otomatis.
    /// </summary>
    private SceneState ResolveSelectedState()
    {

        // Ambil current dari SceneManager
        SceneState current = _sceneManager._currentState;

        // Jika user sudah memilih manual di Inspector → gunakan itu
        if (targetScene != SceneState.None)
            return targetScene;

        // Cari next berdasarkan urutan enum
        SceneState[] values = (SceneState[])System.Enum.GetValues(typeof(SceneState));
        int currentIndex = System.Array.IndexOf(values, current);

        // Fallback aman
        if (currentIndex + 1 >= values.Length)
        {
            Debug.LogWarning("[NextStageController] Tidak ada next scene, fallback ke MainMenu.");
            return SceneState.Menu; // kamu bisa ubah default di sini
        }

        return values[currentIndex];
    }
}
