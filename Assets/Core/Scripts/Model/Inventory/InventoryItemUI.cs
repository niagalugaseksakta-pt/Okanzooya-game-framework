using Inventory.Model;
using System;
using System.Collections.Generic;

namespace Game.Model.Inventory.Struct
{
    [Serializable]
    public struct InventoryItemUI
    {
        public int quantity;
        public ItemSO item;
        public List<ItemParameter> itemState;
        public bool IsEmpty => item == null;

        public InventoryItemUI ChangeQuantity(int newQuantity)
        {
            return new InventoryItemUI
            {
                item = this.item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(this.itemState)
            };
        }

        public static InventoryItemUI GetEmptyItem()
            => new InventoryItemUI
            {
                item = null,
                quantity = 0,
                itemState = new List<ItemParameter>()
            };
    }

}
