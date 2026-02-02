using Inventory.Model;
using UnityEngine;

namespace Game.Model.Data.Player
{
    [CreateAssetMenu(menuName = "SO_PlayerData/Character Data")]
    public class CharacterData : ScriptableObject
    {
        // This Class Holds data for convert and exchange StatBlock
        public string CharacterId;
        public string CharacterName;
        public string CharacterDescription;
        public string CharacterType;

        public int TeamID;
        public Sprite Portrait;

        [Header("Base Stats")]
        public int MaxHealth;
        public int CurrentHealth;

        [Header("Offensive Stats (ATK)")]
        public int AttackPower;   // Physical attack 

        [Header("Defensive Stats (DEF)")]
        public int Defense;       // Physical defense
        public int MagicDefense;  // Magical defense

        [Header("Offensive Stats (MAG)")]
        public int MagicPower;    // Magical attack

        [Header("Agility and Speed (AGI)")]
        public float Agility;     // Controls turn speed, dodge rate
        public float AttackRate;  // Attacks per second
        public float AttackRange;

        [Header("Resources (MANA)")]
        public float Mana;

        [Header("Experience Entity (EXP)")]
        public int Level;
        public int Experience;

        [Header("Criticals")]
        [Tooltip("Chance to land a critical hit (0 to 1)")]
        public float CritChance;
        public float CritMultiplier;

        [Header("Inventory Link")]
        public InventorySO InventoryData;


    }
}
