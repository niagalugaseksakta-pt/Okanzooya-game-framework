using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager _instance { get; private set; }
    private Dictionary<string, string> localizedText;
    private string currentLanguage = "en";
    private bool isReady = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(Application.systemLanguage.ToString().ToLower());
        }
    }

    public static LocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("LocalizationManager");
                _instance = go.AddComponent<LocalizationManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[LocalizationManager] Created persistent instance.");
            }
            return _instance;
        }
    }

    public void LoadLanguage(string languageCode)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, $"lang/{languageCode}.json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Language file not found: {filePath}. Falling back to English.");
            filePath = Path.Combine(Application.streamingAssetsPath, "lang/en.json");
        }

        string dataAsJson = File.ReadAllText(filePath);
        localizedText = JsonUtility.FromJson<LocalizationData>(dataAsJson).ToDictionary();
        currentLanguage = languageCode;
        isReady = true;
    }

    public string Get(string key)
    {
        if (!isReady) return $"[{key}]";
        if (localizedText.TryGetValue(key, out string value))
            return value;
        return $"[{key}]";
    }

    public string CurrentLanguage => currentLanguage;
}
