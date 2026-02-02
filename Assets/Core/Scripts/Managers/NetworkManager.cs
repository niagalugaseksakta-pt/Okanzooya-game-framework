using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _instance { get; private set; }

    [Header("Network Settings")]
    public float DefaultTimeout = 15f; // seconds


    private void Awake()
    {
        // Singleton enforcement
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("[NetworkManager] Duplicate instance detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[NetworkManager] Initialized and persistent.");
    }

    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("NetworkManager");
                _instance = go.AddComponent<NetworkManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[NetworkManager] Created persistent instance.");
            }
            return _instance;
        }
    }
    /// <summary>
    /// Performs a simple GET request and returns the response text.
    /// </summary>
    public async Task<string> Get(string url, Dictionary<string, string> headers = null, CancellationToken token = default)
    {
        using (var request = UnityWebRequest.Get(url))
        {
            if (headers != null)
            {
                foreach (var header in headers)
                    request.SetRequestHeader(header.Key, header.Value);
            }

            request.timeout = Mathf.CeilToInt(DefaultTimeout);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    request.Abort();
                    Debug.LogWarning($"[NetworkManager] GET cancelled: {url}");
                    return null;
                }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkManager] ❌ GET Error ({request.responseCode}): {request.error}");
                return null;
            }

            Debug.Log($"[NetworkManager] ✅ GET {url} ({request.responseCode})");
            return request.downloadHandler.text;
        }
    }

    /// <summary>
    /// Performs a POST request with JSON body and returns the response text.
    /// </summary>
    public async Task<string> Post(string url, string json, Dictionary<string, string> headers = null, CancellationToken token = default)
    {
        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (headers != null)
            {
                foreach (var header in headers)
                    request.SetRequestHeader(header.Key, header.Value);
            }

            request.timeout = Mathf.CeilToInt(DefaultTimeout);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    request.Abort();
                    Debug.LogWarning($"[NetworkManager] POST cancelled: {url}");
                    return null;
                }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkManager] ❌ POST Error ({request.responseCode}): {request.error}");
                return null;
            }

            Debug.Log($"[NetworkManager] ✅ POST {url} ({request.responseCode})");
            return request.downloadHandler.text;
        }
    }
}
