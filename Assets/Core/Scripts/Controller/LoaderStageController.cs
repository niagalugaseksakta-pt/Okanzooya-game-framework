using Game.Model.Player;
using System;
using UnityEngine;
using static Game.Config.Config;

public class LoaderStageController : MonoBehaviour
{
    private SceneScriptManager _sceneManager;

    public bool preloadAdditive = true;

    public static SceneTransitionContext PreparedContext;

    // Plan / Pseudocode:
    // 1. Reinitialize service locator (existing behavior).
    // 2. Try to obtain SaveManager and PlayerEntity from ServiceLocator first, fallback to static Instance / FindObjectOfType.
    // 3. If both SaveManager and playerEntity are available:
    //    a. Call SaveManager.Instance.LoadInto(playerEntity) (safe-guard with try/catch).
    //    b. If playerEntity has a Stats object and Stats.lastCheckpointPosition is non-zero, set playerEntity.transform.position to that value.
    // 4. Log useful warnings when SaveManager or PlayerEntity are missing, but continue initialization.
    // 5. Continue existing initialization: obtain SceneScriptManager, DontDestroyOnLoad, etc.
    private void Awake()
    {
        ServiceLocator.ReInit();
        _sceneManager = ServiceLocator.Get<SceneScriptManager>();
        if (_sceneManager == null)
        {
            Debug.LogError("[LoaderStageController] SceneScriptManager tidak ditemukan dari ServiceLocator.");
        }

        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //test sample


        // Attempt to acquire save manager (via ServiceLocator or static Instance)
        object saveMgrObj = null;
        try
        {
            // Try ServiceLocator first (some projects register singletons there)
            saveMgrObj = ServiceLocator.Get<SaveManager>(); // try a generic get to avoid compile error if SaveManager type is unknown to this file
        }
        catch
        {
            saveMgrObj = null;
        }


        // Try to get a player entity via ServiceLocator or FindObjectOfType
        PlayerEntity playerEntity = null;

        try
        {

            if (playerEntity == null)
            {
                playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
            }

        }
        catch (Exception)
        {
            playerEntity = null;
        }

        // Handling First scenario not find checkpoint after death
        var effectiveSaveManager = (SaveManager)saveMgrObj;

        if (effectiveSaveManager != null && playerEntity != null)
        {
            try
            {
                // Call LoadInto(playerEntity) using reflection to avoid hard compile dependency
                var loadIntoMethod = effectiveSaveManager.GetType().GetMethod("LoadInto", new Type[] { playerEntity.GetType() })
                                     ?? effectiveSaveManager.GetType().GetMethod("LoadInto", new Type[] { typeof(object) });

                if (loadIntoMethod != null)
                {
                    loadIntoMethod.Invoke(effectiveSaveManager, new object[] { playerEntity });
                    Debug.Log("[LoaderStageController] Loaded player data via SaveManager.");
                }
                else
                {
                    Debug.LogWarning("[LoaderStageController] SaveManager found but no suitable LoadInto method signature.");
                }


            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LoaderStageController] Failed to load player save data: " + ex.Message);
            }
        }
        else
        {
            if (effectiveSaveManager == null)
                Debug.Log("[LoaderStageController] No SaveManager.Instance available at Awake; skipping load.");
            if (playerEntity == null)
                Debug.Log("[LoaderStageController] No PlayerEntity found; skipping load.");
        }


        //Handle scene in saved data
        var stats = playerEntity.Stats;
        SceneState next = stats.isFreshStart ? SceneState.Intro : stats.lastScene; // last scene jika checkpoint
        _sceneManager.ChangeState(next);


    }
    //private SceneState ResolveSelectedState()
    //{

    //    // Ambil current dari SceneManager
    //    SceneState current = _scene_manager._currentState;

    //    // Jika user sudah memilih manual di Inspector → gunakan itu
    //    if (targetScene != SceneState.None)
    //        return targetScene;

    //    // Cari next berdasarkan urutan enum
    //    SceneState[] values = (SceneState[])System.Enum.GetValues(typeof(SceneState));
    //    int currentIndex = System.Array.IndexOf(values, current);

    //    // Fallback aman
    //    if (currentIndex + 1 >= values.Length)
    //    {
    //        Debug.LogWarning("[NextStageController] Tidak ada next scene, fallback ke MainMenu.");
    //        return SceneState.Menu; // kamu bisa ubah default di sini
    //    }

    //    return values[currentIndex];
    //}
}
