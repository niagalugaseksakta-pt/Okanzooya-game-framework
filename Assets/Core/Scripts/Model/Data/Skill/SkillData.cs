using UnityEngine;
using static Game.Config.Config;

namespace Game.Model.Data.Skill
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "RPG/Skill")]
    public class SkillAsset : ScriptableObject
    {
        [Header("Basic Info")]
        public string skillId;
        public string skillName;
        public Sprite icon;
        [TextArea] public string description;

        [Header("Stats")]
        public float power;
        public float range;
        public float cooldown;
        public float manaCost;
        public float duration;

        [Header("Behavior")]
        public SkillType skillType;
        public TargetType targetType;
        public AnimationClip animation;
        public AudioClip sfx;

        [Header("Evade/Movement")]
        public bool allowEvade;
        public float evadeDistance;

        [Header("FX")]
        public GameObject effectPrefab;
    }
}
