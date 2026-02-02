using System.Collections;
using UnityEngine;

/// <summary>
/// Handles pre-loading and setup between scene transitions.
/// </summary>
public class LoadingManager : MonoBehaviour
{
    public static LoadingManager _instance { get; private set; }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static LoadingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("LoadingManager");
                _instance = go.AddComponent<LoadingManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[LoadingManager] Created persistent instance.");
            }
            return _instance;
        }
    }

    /// <summary>
    /// Called automatically by SceneLoader before a scene finishes loading.
    /// </summary>
    public IEnumerator PrepareAssets(string sceneName)
    {
        Debug.Log($"[LoadingManager] Preparing assets for scene: {sceneName}");

        // Simulate load resources, config, etc.
        yield return new WaitForSeconds(0.5f);

        // Example: preload UI, audio, or settings
        if (sceneName == "LoadingScene")
        {
            yield return LoadGameAssets();
        }

        Debug.Log($"[LoadingManager] Preparation complete for {sceneName}");
    }

    private IEnumerator LoadGameAssets()
    {
        Debug.Log("[LoadingManager] Loading gameplay assets...");
        yield return new WaitForSeconds(1f); // simulate heavy load
        // You could call AssetBundle loading or Addressable assets here.
    }
}
