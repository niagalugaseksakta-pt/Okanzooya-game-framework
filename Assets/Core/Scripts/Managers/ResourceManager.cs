using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager _instance { get; private set; }

    // Cache for all loaded assets (any type)
    private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ResourceManager");
                _instance = go.AddComponent<ResourceManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[ResourceManager] Created persistent instance.");
            }
            return _instance;
        }
    }

    // 🔹 Load single asset (any type)
    public async Task<T> LoadAsync<T>(string key) where T : UnityEngine.Object
    {
        if (_loadedAssets.TryGetValue(key, out var existing))
            return (T)existing.Result;

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedAssets[key] = handle;
            Debug.Log($"[ResourceManager] ✅ Loaded: {key}");
            return (T)handle.Result;
        }

        Debug.LogError($"[ResourceManager] ❌ Failed to load: {key}");
        return null;
    }

    // 🔹 Load all assets with a specific label (batch preload)
    public async Task<IList<T>> LoadByLabelAsync<T>(string label) where T : UnityEngine.Object
    {
        Debug.Log($"[ResourceManager] 🔍 Loading by label: {label}");

        var handle = Addressables.LoadAssetsAsync<T>(label, null);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[ResourceManager] ❌ Failed to load label: {label}");
            return null;
        }

        foreach (var obj in handle.Result)
        {
            string key = $"{label}_{obj.name}";
            if (!_loadedAssets.ContainsKey(key))
                _loadedAssets[key] = handle;
        }

        Debug.Log($"[ResourceManager] ✅ Loaded {handle.Result.Count} assets for label '{label}'");
        return handle.Result;
    }

    // 🔹 Check if an asset is already loaded
    public bool IsLoaded(string key)
    {
        return _loadedAssets.TryGetValue(key, out var handle)
            && handle.IsValid()
            && handle.Status == AsyncOperationStatus.Succeeded;
    }

    // 🔹 Retrieve cached asset (any type)
    public T Get<T>(string key) where T : UnityEngine.Object
    {
        if (IsLoaded(key))
            return (T)_loadedAssets[key].Result;

        Debug.LogWarning($"[ResourceManager] ⚠️ Asset not loaded: {key}");
        return null;
    }

    // 🔹 Release a specific asset from cache
    public void Release(string key)
    {
        if (_loadedAssets.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _loadedAssets.Remove(key);
            Debug.Log($"[ResourceManager] 🧹 Released: {key}");
        }
    }

    // 🔹 Release all cached assets
    public void ReleaseAll()
    {
        foreach (var kvp in _loadedAssets)
        {
            Addressables.Release(kvp.Value);
            Debug.Log($"[ResourceManager] 🧹 Released: {kvp.Key}");
        }
        _loadedAssets.Clear();
    }
}
