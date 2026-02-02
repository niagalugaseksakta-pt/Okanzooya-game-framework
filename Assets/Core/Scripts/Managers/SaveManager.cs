using Game.Config;
using Game.Model;
using Game.Model.Player;
using Inventory.Model;
using System;
using System.IO;
using System.Text;
using UnityEngine;


public class SaveManager : MonoBehaviour
{
    public static SaveManager _instance { get; private set; }

    private EntityDataModel data;

    // 🔹 Optional callback for save/load events (handy for UI or debugging)
    public event Action OnSaveComplete;
    public event Action OnLoadComplete;
    public event Action OnDeleteComplete;

    // ──────────────────────────────────────────────────────────────
    #region Initialization

    private void Awake()
    {

        // Check if the file exists
        if (!File.Exists(Config.savePath))
        {
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(Config.savePath));

            // Create an empty or default save file
            using (FileStream fs = File.Create(Config.savePath))
            {
                // Optionally write some default data (like JSON, or version info)
                byte[] defaultData = System.Text.Encoding.UTF8.GetBytes("{\"version\":1,\"data\":{}}");
                fs.Write(defaultData, 0, defaultData.Length);
            }

            Debug.Log($"Save file created at: {Config.savePath}");
        }
        else
        {
            Debug.Log($"Save file already exists at: {Config.savePath}");
        }
        DontDestroyOnLoad(gameObject);
    }

    public static void Init()
    {
        if (_instance == null)
        {
            _instance = new SaveManager();
            Debug.Log($"[SaveManager] Initialized ✅ Path: {Config.savePath}");
        }
    }

    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SaveManager");
                _instance = go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[SaveManager] Created persistent instance.");
            }
            return _instance;
        }
    }
    #endregion

    // ──────────────────────────────────────────────────────────────
    #region Save / Load
    public void Save(PlayerEntity entity)
    {
        try
        {
            if (entity == null)
            {

                // 1. Try by TAG (fast + explicit)
                GameObject go = GameObject.FindWithTag("Player");

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

                // 3. Fallback: by TYPE (slow but reliable)
                if (go == null)
                    go = FindAnyObjectByType<PlayerEntity>()?.gameObject;

                // Assign
                if (go != null)
                    entity = go.GetComponent<PlayerEntity>();
                else
                    Debug.LogError("[DialogueController] Player not found! Check TAG/LAYER/COMPONENT.");

                Debug.LogWarning("[SaveManager] Doing reload entity.");
                return;
            }

            string json = JsonUtility.ToJson(entity.ToData(), prettyPrint: false);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            GameSpellDefender<EncryptionManager>.Secure(() =>
            {
                var enc = new EncryptionManager();
                byte[] encrypted = enc.EncryptBytes(jsonBytes);
                File.WriteAllBytes(Config.savePath, encrypted);
            });

            OnSaveComplete?.Invoke();
            Debug.Log($"💾 [SaveManager] Saved: {entity.DisplayName} (encrypted at {Config.savePath})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ [SaveManager] Save failed: {ex.Message}");
        }
    }

    public void LoadInto(EntityBase entity)
    {
        try
        {
            if (!File.Exists(Config.savePath))
            {
                Debug.Log("[SaveManager] ⚠️ No save file found.");
                return;
            }

            GameSpellDefender<EncryptionManager>.Secure(() =>
            {
                var enc = new EncryptionManager();
                byte[] encrypted = File.ReadAllBytes(Config.savePath);
                byte[] decrypted = enc.DecryptBytes(encrypted);

                string json = Encoding.UTF8.GetString(decrypted);
                data = JsonUtility.FromJson<EntityDataModel>(json);

                entity.FromData(data);
                OnLoadComplete?.Invoke();
                Debug.Log($"📥 [SaveManager] Loaded data for: {entity.DisplayName}");
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ [SaveManager] Load failed: {ex.Message}");
        }
    }

    public void LoadInventoryData(EntityBase entity)
    {
        try
        {

            LoadInto(entity); // First Load Player Data from saved file

            if (entity.Stats.InventoryData == null)
            {
                entity.Stats.InventoryData = Resources.Load<InventorySO>("InventoryData/PlayerInventory");
                Debug.Log("📦 InventorySO reattached from Resources folder.");
            }

            //When Loaded, we need to fill Inventory Data
            entity.Stats.InventoryData.InformAboutChange();

        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to load inventory: {ex}");

        }
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    #region Delete / Utilities

    /// <summary>
    /// Deletes the current save file if it exists.
    /// </summary>
    public void DeleteSave()
    {
        try
        {
            if (File.Exists(Config.savePath))
            {
                File.Delete(Config.savePath);
                data = null;
                OnDeleteComplete?.Invoke();
                Debug.Log($"🗑️ [SaveManager] Save file deleted at {Config.savePath}");
            }
            else
            {
                Debug.Log("[SaveManager] No save file found to delete.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ [SaveManager] Delete failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a save file currently exists.
    /// </summary>
    public bool HasSave() => File.Exists(Config.savePath);


    #endregion

    // ──────────────────────────────────────────────────────────────
    public EntityDataModel GetData() => data;
}
