using Inventory.Model;
using System;
using UnityEngine;
using static Game.Config.Config;

namespace Game.Model.Struct
{
    [Serializable]
    public class StatBlock
    {
        [Header("Coin, Level, Diamond")]
        public int Coin = 1;
        public int Level = 1;
        public int Diamond = 1;

        [Header("Base Stats")]
        public int MaxHealth = 1;
        public int CurrentHealth = 1;

        [Header("Offensive Stats (ATK)")]
        public int AttackPower = 1;   // Physical attack 

        [Header("Defensive Stats (DEF)")]
        public int Defense = 1;       // Physical defense
        public int MagicDefense = 1;  // Magical defense

        [Header("Offensive Stats (MAG)")]
        public int MagicPower = 1;    // Magical attack

        [Header("Agility and Speed (AGI)")]
        public float Agility = 1;     // Controls turn speed, dodge rate
        public float AttackRate = 1;  // Attacks per second
        public float AttackRange = 1;

        [Header("Resources (MANA)")]
        public float Mana = 1;

        [Header("Experience Entity (EXP)")]
        public int Experience = 1;

        [Header("Criticals")]
        [Tooltip("Chance to land a critical hit (0 to 1)")]
        public float CritChance = 1;
        public float CritMultiplier = 1;

        [Header("Inventory Link")]
        public InventorySO InventoryData;

        [Header("CheckPointStateAndScene")]
        public Vector3 lastCheckpointPosition;
        public string checkpointID;
        public SceneState lastScene;
        public bool isFreshStart = true;
        public bool isThroughDoor = false;

        // Portal or door related stats
        public int lastPortalId; // example 0
        public string lastPortalNamebyScene;
        public Vector3 lastPortalpositionsSpawner;
        public int currentPortalId; // example 1
        public string currentPortalNamebyScene;
        public Vector3 currentPortalpositionsSpawner;

        public PortalState nextPortal; // example 1

        // Derived Calculations
        public int CalculatePhysicalDamage()
        {
            bool crit = UnityEngine.Random.value < CritChance;
            float modifier = crit ? CritMultiplier : 1f;
            return Mathf.RoundToInt(AttackPower * modifier);
        }

        public int CalculateMagicalDamage()
        {
            bool crit = UnityEngine.Random.value < CritChance * 0.5f;
            float modifier = crit ? CritMultiplier : 1f;
            return Mathf.RoundToInt(MagicPower * modifier);
        }

        public int ApplyPhysicalDefense(int damage)
        {
            float reduction = Mathf.Clamp01(Defense / 200f);
            return Mathf.Max(1, Mathf.RoundToInt(damage * (1f - reduction)));
        }

        public int ApplyMagicalDefense(int damage)
        {
            float reduction = Mathf.Clamp01(MagicDefense / 200f);
            return Mathf.Max(1, Mathf.RoundToInt(damage * (1f - reduction)));
        }
    }
}