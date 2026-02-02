using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "RPG/Parameter/ItemEdibleOrEquipment")]
    public class ItemParameterSO : ScriptableObject
    {
        [field: SerializeField]
        public string ParameterName { get; private set; }
    }
}