using UnityEngine;
using UnityEngine.SceneManagement;

public class FinderTagHelper : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
        PSEUDOCODE / PLAN (detailed):
        1. Provide helper local functions to locate a GameObject by tag across scenes and hierarchies:
            - FindInHierarchy(GameObject root, string tag): recursively search root and children for tag, return GameObject or null.
            - FindInSceneByTag(Scene scene, string tag): iterate scene.GetRootGameObjects() and call FindInHierarchy on each root.
            - FindAcrossScenes(string tag): iterate all loaded scenes and call FindInSceneByTag.
            - FindInUiCanvasAcrossScenes(string tag): for each scene, find root GameObjects named "UiCanvas", then search within them using FindInHierarchy.
            - FindTagged(string tag): top-level lookup which:
                a) attempts to search inside the scene named "coreinterfacetouser" if it exists and is loaded,
                b) attempts to search inside any "UiCanvas" across scenes,
                c) attempts a full search across all loaded scenes,
                d) falls back to GameObject.FindWithTag for backward compatibility.
        2. Use FindTagged for each UI tag lookup used previously:
            - "TagPanelDialougeAnjing" -> panelDialogue
            - "TitleText" -> titleText (get TextMeshProUGUI)
            - "DialogueText" -> dialogueText (get TextMeshProUGUI)
            - "NextDialogue" -> nextIndicator
            - "MainButton" -> mainButtonUI
            - "PanelButtonInteraction" -> interactButtonUI
            - "ButtonInteraction" -> interactButton (get Button)
            - "PanelChoiceContainer" -> choiceContainer (transform)
        3. Keep the rest of Awake intact: player search fallback, component setup (animator/audio), UI safety, button hookup, and warnings.
        4. Use fully qualified UnityEngine.SceneManagement types so no using directive is required here.
        */

    // Local helpers
    public static GameObject FindInHierarchy(GameObject root, string tag)
    {
        if (root == null) return null;
        try
        {
            if (root.CompareTag(tag))
                return root;
        }
        catch { /* ignore tag compare failures */ }

        foreach (Transform child in root.transform)
        {
            var found = FindInHierarchy(child.gameObject, tag);
            if (found != null) return found;
        }
        return null;
    }

    public static GameObject FindInSceneByTag(UnityEngine.SceneManagement.Scene scene, string tag)
    {
        if (!scene.IsValid() || !scene.isLoaded) return null;
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var found = FindInHierarchy(root, tag);
            if (found != null) return found;
        }
        return null;
    }
    public static GameObject FindInActiveSceneByTag(string tag)
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return null;
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var found = FindInHierarchy(root, tag);
            if (found != null) return found;
        }
        return null;
    }

    public static GameObject FindInUiCanvasAcrossScenes(string tag)
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded) continue;
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                if (string.Equals(root.name, "UiCanvas", System.StringComparison.OrdinalIgnoreCase))
                {
                    var found = FindInHierarchy(root, tag);
                    if (found != null) return found;
                }
            }
        }
        return null;
    }

    public static GameObject FindAcrossScenes(string tag)
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            var found = FindInSceneByTag(scene, tag);
            if (found != null) return found;
        }
        return null;
    }

    public static GameObject FindTagged(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;

        // 1) Try the specific scene "coreinterfacetouser" first
        var targetScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("coreinterfacetouser");
        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            var found = FindInSceneByTag(targetScene, tag);
            if (found != null) return found;
        }

        // 2) Try searching inside any UiCanvas across loaded scenes (useful when UI is hosted in a separate scene)
        var foundInCanvas = FindInUiCanvasAcrossScenes(tag);
        if (foundInCanvas != null) return foundInCanvas;

        // 3) Try scanning all loaded scenes
        var foundAll = FindAcrossScenes(tag);
        if (foundAll != null) return foundAll;

        // 4) Fallback to default (active-only) FindWithTag for backwards compatibility
        try
        {
            return GameObject.FindWithTag(tag);
        }
        catch
        {
            return null;
        }
    }

    public static GameObject FindPlayer<T>()
    {
        // 1. Try by TAG (fast + explicit)
        GameObject go = null;
        try
        {
            go = GameObject.FindWithTag("Player");
        }
        catch { go = null; }

        // 2. Try by LAYER (if tag failed)
        if (go == null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (obj.layer == playerLayer)
                    {
                        go = obj;
                        break;
                    }
                }
            }
        }

        // Assign
        if (go != null)
            return go;
        else
            Debug.LogError("[DialogueController] Player not found! Check TAG/LAYER/COMPONENT.");

        return null;
    }
}
