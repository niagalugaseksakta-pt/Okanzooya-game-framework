using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "RPG/Equipable/Equipment")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private List<ModifierData> modifiersData = new List<ModifierData>();
        public string ActionName => "Equip";

        [field: SerializeField]
        public AudioClip actionSFX { get; private set; }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            foreach (ModifierData data in modifiersData)
            {
                data.statModifier.AffectCharacter(character, data.value);
            }
            return true;

            //AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            //if (weaponSystem != null)
            //{
            //    weaponSystem.SetWeapon(this, itemState == null ?
            //        DefaultParametersList : itemState);
            //    return true;
            //}
            //return false;
        }
    }
}