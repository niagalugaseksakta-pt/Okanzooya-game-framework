using Game.Config;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class EncryptionManager : MonoBehaviour, IEncryptionManager
{
    public static EncryptionManager _instance { get; private set; }

    [Header("Encryption Settings")]
    [SerializeField] private string encryptionKey = "default-key"; // Optional: can override via inspector

    private Aes _aes;
    private readonly byte xorKey = 0x5A; // Simple XOR key (demo only)

    private void Awake()
    {
        // Singleton enforcement
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Load key from config if available
        if (!string.IsNullOrEmpty(Config.key))
            encryptionKey = Config.key;

        Debug.Log($"[EncryptionManager] Initialized with key length: {encryptionKey.Length}");
    }

    public static EncryptionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("EncryptionManager");
                _instance = go.AddComponent<EncryptionManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[EncryptionManager] Created persistent instance.");
            }
            return _instance;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public void BeginSession()
    {
        _aes = Aes.Create();
        _aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
        _aes.IV = new byte[16];
        Debug.Log("[EncryptionManager] Session started");
    }

    public void EndSession()
    {
        _aes?.Dispose();
        _aes = null;
        Debug.Log("[EncryptionManager] Session ended");
    }

    public bool VerifyKey()
    {
        return !string.IsNullOrEmpty(encryptionKey) && encryptionKey.Length > 8;
    }

    public string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16];

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string DecryptString(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }

    public void SaveEncryptedToFile(string fileName, string plainText)
    {
        try
        {
            string encrypted = EncryptString(plainText);
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, encrypted);
            Debug.Log($"[EncryptionManager] Saved encrypted data to {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EncryptionManager] Save failed: {ex.Message}");
        }
    }

    public string LoadDecryptedFromFile(string fileName)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(path)) return string.Empty;

            string encrypted = File.ReadAllText(path);
            return DecryptString(encrypted);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EncryptionManager] Load failed: {ex.Message}");
            return string.Empty;
        }
    }

    public byte[] EncryptBytes(byte[] data)
    {
        byte[] buffer = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
            buffer[i] = (byte)(data[i] ^ xorKey);
        return buffer;
    }

    public byte[] DecryptBytes(byte[] data)
    {
        // XOR again = decrypt
        return EncryptBytes(data);
    }
}
