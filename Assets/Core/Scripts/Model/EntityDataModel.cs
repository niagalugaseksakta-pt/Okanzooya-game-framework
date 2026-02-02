using Game.Model.Struct;
using UnityEngine;

namespace Game.Model
{
    [System.Serializable]
    public class EntityDataModel
    {
        // To Data Model from EntityBase and its derived classes
        // Entity Property
        public string Id;
        public string DisplayName;
        public int TeamId;
        public Vector3 Position;
        public Quaternion Rotation;
        public Transform AttachedTransform;
        public bool UseAttachedAsPrimary = false;
        public StatBlock Stats;
        public int Score = 0;
        public bool IsInvulnerable;
        public bool IsDead = false;
        public string LocalizationKey;
        public bool IsHaveSpellCast = false;
        public bool IsTurn = false;


    }
}