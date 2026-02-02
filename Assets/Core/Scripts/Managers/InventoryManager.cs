using Game.Model.Inventory.Struct;
using Game.Model.Player;
using Inventory.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
//UI or Bag
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager _instance { get; private set; }

    [Header("Item Database")]
    [Tooltip("Optional: All possible items in the game (ScriptableObjects).")]
    [SerializeField] private PlayerEntity playerEntity;

    [Header("Inventory Regions")]
    [Tooltip("Must be integrated and add for manager works")]
    [SerializeField]
    private List<InventoryItemUI> _inventory = new();

    [SerializeField]
    private InventorySO inventoryData;


    private SaveManager saveManager;
    public event Action OnInventoryChanged;


    //Example Button
    [SerializeField] private Button Button;
    [SerializeField] private ItemSO itemDataSO;
    private void Awake()
    {
        //corrupted

        _instance = this;
        DontDestroyOnLoad(gameObject);

        //Example
        //Button.onClick.AddListener(() => AddItem(itemDataSO, 1, null));


    }

    private void FillPlayerEntity()
    {
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }
    }

    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("InventoryManager");
                _instance = go.AddComponent<InventoryManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[InventoryManager] Created persistent instance.");
            }
            return _instance;
        }
    }

    #region === Core Inventory Methods ===

    public void AddItem(InventoryItemUI item)
    {
        AddItem(item.item, item.quantity);
    }

    public void AddItem(ItemSO items, int quantitys, List<ItemParameter> itemState = null)
    {
        inventoryData.AddItem(items, quantitys, itemState);

        Debug.Log($"✅ Added {quantitys}x {items.name}");
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(int itemIndex, int amount)
    {
        inventoryData.RemoveItem(itemIndex, amount);

        Debug.Log($"🗑️ Removed {itemIndex}x amount = {amount}");
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(int itemId) =>
        _inventory.Any(i => i.item.ID == itemId && i.quantity > 0);

    public int GetQuantity(int itemId) =>
        _inventory.FirstOrDefault(i => i.item.ID == itemId).quantity != 0 ? _inventory.FirstOrDefault(i => i.item.ID == itemId).quantity : 0;

    public List<InventoryItemUI> GetAllItems() => _inventory;

    public void ClearInventory()
    {
        _inventory.Clear();
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }

    #endregion

    #region === Save / Load ===

    public void SaveInventory()
    {
        try
        {
            FillPlayerEntity();
            //var stats = playerEntity.Stats;

            //playerEntity.SetStats(stats);

            SaveManager.Instance.Save(playerEntity);
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to save inventory: {ex}");
        }
    }

    public void LoadInventoryData()
    {
        try
        {
            FillPlayerEntity();
            SaveManager.Instance.LoadInto(playerEntity); // First Load Player Data from saved file

            if (playerEntity.Stats.InventoryData == null)
            {
                playerEntity.Stats.InventoryData = Resources.Load<InventorySO>("InventoryData/PlayerInventory");
                Debug.Log("📦 InventorySO reattached from Resources folder.");
            }

            //When Loaded, we need to fill Inventory Data
            playerEntity.Stats.InventoryData.ReInitializeByUpdate();

        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to load inventory: {ex}");
            _inventory = new List<InventoryItemUI>();
        }
    }


    #endregion

    #region === Utility ===


    #endregion
}
