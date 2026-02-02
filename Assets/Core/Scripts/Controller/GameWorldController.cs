using Game.Model.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameWorldController : MonoBehaviour
{
    private PlayerEntity playerEntity;
    private void Awake()
    {
        // Subscribe to scene events
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        // Initialize existing loaded scenes based on current active scene
        RefreshAllScenesGameWorlds();

        // Get player entity and set initial position

        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid leaks
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        // When active scene changes, enable GameWorlds in the new active scene and
        // disable GameWorlds in all other loaded scenes.
        RefreshAllScenesGameWorlds();
        SetAndSavePlayerPositions();
    }

    private void SetAndSavePlayerPositions()
    {
        if (playerEntity == null)
            return; // abort if still missing

        //Handle first playing first scene
        if (playerEntity.Stats.lastCheckpointPosition == Vector3.zero)
        {
            playerEntity.transform.position = GameObject.FindWithTag("ManualSpawn").transform.position;
            return;
        }

        playerEntity.Stats.lastCheckpointPosition = playerEntity.Stats.isThroughDoor ? PortalRegistry.Get(playerEntity.Stats.nextPortal).transform.position : playerEntity.Stats.lastCheckpointPosition;
        playerEntity.Stats.isFreshStart = playerEntity.Stats.isThroughDoor ? false : true;

        playerEntity.transform.position = playerEntity.Stats.lastCheckpointPosition;
        try
        {
            SaveManager._instance.Save(playerEntity);
        }
        catch
        {
            Debug.LogWarning("[Checkpoint] SaveManager belum di-assign.");
        }
    }

    private void RefreshAllScenesGameWorlds()
    {
        Scene active = SceneManager.GetActiveScene();
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.IsValid()) continue;
            bool shouldBeActive = s == active;
            SetGameWorldsActiveInScene(s, shouldBeActive);
        }
    }

    private void SetGameWorldsActiveInScene(Scene scene, bool active)
    {
        foreach (var gw in FindGameWorldsInScene(scene))
        {
            if (gw != null && gw.activeSelf != active)
            {
                gw.SetActive(active);
            }
        }
    }

    private IEnumerable<GameObject> FindGameWorldsInScene(Scene scene)
    {
        if (!scene.IsValid()) yield break;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            foreach (var found in FindGameWorldsInHierarchy(roots[i].transform))
            {
                yield return found;
            }
        }
    }

    private IEnumerable<GameObject> FindGameWorldsInHierarchy(Transform root)
    {
        // Use a stack to avoid recursion
        var stack = new Stack<Transform>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var t = stack.Pop();
            if (t.name == "GameWorld")
            {
                yield return t.gameObject;
            }

            for (int i = 0; i < t.childCount; i++)
            {
                stack.Push(t.GetChild(i));
            }
        }
    }
}
