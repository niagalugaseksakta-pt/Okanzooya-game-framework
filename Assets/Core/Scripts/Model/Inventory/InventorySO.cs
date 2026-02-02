using Game.Model.Inventory.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Core Memory Heap Database
namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "InventoryDatabase", menuName = "RPG/Inventory/Inventory")]
    [Serializable]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<InventoryItemUI> inventoryItemUIs;

        [field: SerializeField]
        public int Size { get; private set; } = 10; // Set Empty Slot without loaded Items

        public event Action<Dictionary<int, InventoryItemUI>> OnInventoryUpdated;

        private void EnsureInitialized()
        {
            if (inventoryItemUIs == null || inventoryItemUIs.Count != Size)
            {
                inventoryItemUIs = new List<InventoryItemUI>(Size);
                for (int i = 0; i < Size; i++)
                {
                    inventoryItemUIs.Add(InventoryItemUI.GetEmptyItem());
                }
            }
        }

        public void Initialize()
        {
            inventoryItemUIs = new List<InventoryItemUI>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItemUIs.Add(InventoryItemUI.GetEmptyItem());
            }
        }

        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            EnsureInitialized();

            if (item == null)
                return quantity;

            if (item.IsStackable == false)
            {
                for (int i = 0; i < inventoryItemUIs.Count; i++)
                {
                    while (quantity > 0 && IsInventoryFull() == false)
                    {
                        quantity -= AddItemToFirstFreeSlot(item, 1, itemState);
                    }
                    InformAboutChange();
                    return quantity;
                }
            }
            quantity = AddStackableItem(item, quantity);
            InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSO item, int quantity
            , List<ItemParameter> itemState = null)
        {
            EnsureInitialized();

            InventoryItemUI newItem = new InventoryItemUI
            {
                item = item,
                quantity = quantity,
                itemState =
                new List<ItemParameter>(itemState == null ? item.DefaultParametersList : itemState)
            };

            for (int i = 0; i < inventoryItemUIs.Count; i++)
            {
                if (inventoryItemUIs[i].IsEmpty)
                {
                    inventoryItemUIs[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull()
        {
            EnsureInitialized();
            return inventoryItemUIs.Where(item => item.IsEmpty).Any() == false;
        }

        private int AddStackableItem(ItemSO item, int quantity)
        {
            EnsureInitialized();

            for (int i = 0; i < inventoryItemUIs.Count; i++)
            {
                if (inventoryItemUIs[i].IsEmpty)
                    continue;
                if (inventoryItemUIs[i].item.ID == item.ID)
                {
                    int amountPossibleToTake =
                        inventoryItemUIs[i].item.MaxStackSize - inventoryItemUIs[i].quantity;

                    if (quantity > amountPossibleToTake)
                    {
                        inventoryItemUIs[i] = inventoryItemUIs[i]
                            .ChangeQuantity(inventoryItemUIs[i].item.MaxStackSize);
                        quantity -= amountPossibleToTake;
                    }
                    else
                    {
                        inventoryItemUIs[i] = inventoryItemUIs[i]
                            .ChangeQuantity(inventoryItemUIs[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }
            while (quantity > 0 && IsInventoryFull() == false)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item, newQuantity);
            }
            return quantity;
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            EnsureInitialized();

            if (itemIndex < 0 || itemIndex >= inventoryItemUIs.Count)
                return;

            if (inventoryItemUIs[itemIndex].IsEmpty)
                return;
            int reminder = inventoryItemUIs[itemIndex].quantity - amount;
            if (reminder <= 0)
                inventoryItemUIs[itemIndex] = InventoryItemUI.GetEmptyItem();
            else
                inventoryItemUIs[itemIndex] = inventoryItemUIs[itemIndex]
                    .ChangeQuantity(reminder);

            InformAboutChange();
        }

        public void AddItem(InventoryItemUI item)
        {
            EnsureInitialized();

            if (item.IsEmpty || item.item == null) return;
            AddItem(item.item, item.quantity);
        }

        public InventoryItemUI GetItemAt(int itemIndex)
        {
            EnsureInitialized();

            if (itemIndex < 0 || itemIndex >= inventoryItemUIs.Count)
                return InventoryItemUI.GetEmptyItem();

            var res = inventoryItemUIs[itemIndex];
            return res.IsEmpty ? InventoryItemUI.GetEmptyItem() : res;
        }

        public void SwapItems(int itemIndex_1, int itemIndex_2) //index 1 is dragged | 2 is to drop
        {
            EnsureInitialized();

            if (itemIndex_1 < 0 || itemIndex_1 >= inventoryItemUIs.Count ||
                itemIndex_2 < 0 || itemIndex_2 >= inventoryItemUIs.Count)
                return;

            var itemUI1 = inventoryItemUIs[itemIndex_1];
            var itemUI2 = inventoryItemUIs[itemIndex_2];

            if (itemUI1.IsEmpty && itemUI2.IsEmpty)
                return;

            // if same item type and stackable, merge instead of swapping
            if (!itemUI1.IsEmpty && !itemUI2.IsEmpty && itemUI1.item.ID == itemUI2.item.ID && itemUI1.item.IsStackable && itemUI2.item.IsStackable)
            {
                MergeStackableItems(itemIndex_1, itemIndex_2);
            }
            else
            {
                // normal swap
                NormalSwap(itemIndex_1, itemIndex_2);
            }

            InformAboutChange();
        }

        private void NormalSwap(int itemIndex_1, int itemIndex_2)
        {
            EnsureInitialized();

            InventoryItemUI item1 = inventoryItemUIs[itemIndex_1];
            inventoryItemUIs[itemIndex_1] = inventoryItemUIs[itemIndex_2];
            inventoryItemUIs[itemIndex_2] = item1;
        }

        /// <summary>
        /// Merge quantities between two stackable slots up to max stack (default 99)
        /// </summary>
        private void MergeStackableItems(int index1, int index2)
        {
            var slot1 = inventoryItemUIs[index1];
            var slot2 = inventoryItemUIs[index2];

            int total = slot1.quantity + slot2.quantity;

            // decide which slot to fill to max first (optional priority logic)
            // Here we fill slot2 first, then remainder stays in slot1
            if (total <= 99) // Min
            {
                // All fit in one slot

                // Find Biggest
                //if (slot1.quantity > slot2.quantity) // if dragged is bigger than drop
                //{
                //    // dragger keeps total
                //    InventoryItemUIs[index1] = InventoryItemUIs[index1].ChangeQuantity(total);
                //    InventoryItemUIs[index2] = InventoryItemUIs[index2].ChangeQuantity(0);

                //}
                //else if(slot1.quantity == slot2.quantity)
                //{
                // set drop to add value
                inventoryItemUIs[index1] = inventoryItemUIs[index1].ChangeQuantity(0);
                inventoryItemUIs[index2] = inventoryItemUIs[index2].ChangeQuantity(total);
                //}
                //else 
                //{
                //    // set drop to add value
                //    InventoryItemUIs[index1] = InventoryItemUIs[index1].ChangeQuantity(0);
                //    InventoryItemUIs[index2] = InventoryItemUIs[index2].ChangeQuantity(total);
                //}


            }
            else // Max
            {

                // Find Biggest
                //if (slot1.quantity > slot2.quantity)
                //{
                //    InventoryItemUIs[index1] = InventoryItemUIs[index1].ChangeQuantity(total - 99);
                //    InventoryItemUIs[index2] = InventoryItemUIs[index2].ChangeQuantity(99);
                //}
                //else if (slot1.quantity == slot2.quantity)
                //{
                inventoryItemUIs[index1] = inventoryItemUIs[index1].ChangeQuantity(99);
                inventoryItemUIs[index2] = inventoryItemUIs[index2].ChangeQuantity(total - 99);
                //}
                //else
                //{
                //    InventoryItemUIs[index1] = InventoryItemUIs[index1].ChangeQuantity(99);
                //    InventoryItemUIs[index2] = InventoryItemUIs[index2].ChangeQuantity(total - 99);
                //}


            }

            // if one became empty, you can clear that slot
            if (inventoryItemUIs[index1].quantity == 0)
                RemoveItem(index1, 100); // Handle remove item over max stack :D
            if (inventoryItemUIs[index2].quantity == 0)
                RemoveItem(index2, 100);
        }


        public void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(ReInitializeByUpdate()); //Rebuild New Dictionary to heap
        }

        public Dictionary<int, InventoryItemUI> GetCurrentInventoryState()
        {
            EnsureInitialized();

            Dictionary<int, InventoryItemUI> returnValue =
                new Dictionary<int, InventoryItemUI>();

            for (int i = 0; i < inventoryItemUIs.Count; i++)
            {
                if (inventoryItemUIs[i].IsEmpty)
                    continue;
                returnValue[i] = inventoryItemUIs[i];
            }
            return returnValue;
        }

        public Dictionary<int, InventoryItemUI> ReInitializeByUpdate()
        {
            EnsureInitialized();

            Dictionary<int, InventoryItemUI> returnValue =
                new Dictionary<int, InventoryItemUI>();

            for (int i = 0; i < inventoryItemUIs.Count; i++)
            {
                if (inventoryItemUIs[i].IsEmpty)
                {
                    returnValue[i] = InventoryItemUI.GetEmptyItem();
                }
                else
                {
                    returnValue[i] = inventoryItemUIs[i];
                }

            }
            return returnValue;
        }
    }

}
