using Game.Model.Data.Player;
using Game.Model.Struct;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game.Config.Config;

namespace Game.Model
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [Serializable]
    public abstract class EntityBase : MonoBehaviour
    {

        [Header("Entity Identity")]
        [SerializeField, Tooltip("Unique runtime identifier")]
        protected string characterId = Guid.NewGuid().ToString();
        [SerializeField, Tooltip("Display name for debugging/UI")]
        protected string characterName = " Entity ";
        [SerializeField]
        protected string characterDescription;
        [SerializeField]
        protected string characterType;

        [SerializeField, Tooltip("Team or faction ID (e.g., 0=Neutral, 1=Player, 2=Enemy)")]
        protected int teamId = 0;

        [Header("World State")]
        [SerializeField] protected Vector3 position;
        [SerializeField] protected Quaternion rotation;

        [Header("Attachment")]
        [Tooltip("Optional child transform (e.g. model, visual root) that follows this entity")]
        public Transform attachedTransform;

        [Tooltip("Should the attached transform drive the main position?")]
        public bool useAttachedAsPrimary = false;

        [Header("Core Stats")]
        [SerializeField] protected StatBlock stats;

        [Header("Gameplay Stats")]
        [SerializeField] protected int score = 0;
        [SerializeField] protected bool isInvulnerable = false; // Kek Mirip Keracunan
        [SerializeField] protected bool isDead = false;

        [Header("Localization")]
        [SerializeField] protected string localizationKey;

        [Header("Entity Behaviour")]
        [SerializeField] protected bool isHaveSpellCast = false;
        [SerializeField] protected bool isTurn = false;

        [Header("Hit Blink")]
        protected SpriteRenderer sprite;
        [SerializeField] protected Color blinkColor = Color.red;
        [SerializeField] private float blinkDuration = 0.1f;
        [SerializeField] protected Color originalColor = Color.white;
        private Coroutine blinkRoutine;

        [Header("Debug")]
        [SerializeField] protected bool showDebugLogs = true;

        protected Dictionary<EntityActionType, string> animationMap = new();
        protected Animator animator;
        protected Coroutine animationCoroutine;
        public bool IsRecentlyDamaged;

        // --- Events ---
        public event Action<int> OnDamagedEvent;
        public event Action OnDeathEvent;
        public event Action<int> OnHealedEvent;
        public event Action OnLevelUpEvent;

        // --- Properties ---
        public string Id => characterId;
        public string DisplayName => characterName;
        public Vector3 Position => position;
        public Quaternion Rotation => rotation;
        public int Score => score;
        public int TeamID => teamId;
        public string LocalizationKey => localizationKey;
        public StatBlock Stats => stats;
        public bool IsInvulnerable => isInvulnerable;
        public bool IsDead => isDead;
        public bool IsHaveSpellCast => isHaveSpellCast;
        public bool IsTurn => isTurn;


        // --- Unity Lifecycle ---
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(characterId))
                characterId = Guid.NewGuid().ToString();

            position = transform.position;
            rotation = transform.rotation;

            stats.CurrentHealth = stats.MaxHealth;
            isDead = false;

            RegisterDefaultAnimations();

            if (!attachedTransform)
            {
                // Auto-assign if child exists
                Transform model = transform.Find("Model");
                if (model != null) attachedTransform = model;
            }


        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            // 🔄 Sync between entity and attached transform (Projectile)
            if (useAttachedAsPrimary && attachedTransform)
            {
                position = attachedTransform.position;
                rotation = attachedTransform.rotation;
                transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                position = transform.position;
                rotation = transform.rotation;
                if (attachedTransform)
                    attachedTransform.SetPositionAndRotation(position, rotation);
            }
        }

        #region "Expression"
        protected virtual void ShowAlertEmote(EmoteState emoteState)
        {
            Debug.Log($"[{this.gameObject.name + characterName}] shows alert emote {emoteState}!");
        }
        #endregion

        #region "Animation Management"
        /// <summary>
        /// Each subclass overrides to define its own animation mapping.
        /// </summary>
        protected virtual void RegisterDefaultAnimations()
        {
            animationMap[EntityActionType.Idle] = "Idle";
            animationMap[EntityActionType.Walk] = "Walk";
            animationMap[EntityActionType.Attack] = "Attack";
            animationMap[EntityActionType.Die] = "Die";
        }

        /// <summary>
        /// High-level call for animation through logical action type.
        /// </summary>
        public virtual void PlayAction(EntityActionType action)
        {
            if (animator == null)
            {
                Debug.LogWarning($"[{name}] Animator missing.");
                return;
            }

            if (!animationMap.TryGetValue(action, out var clip))
            {
                Debug.LogWarning($"[{name}] has no animation for action {action}.");
                return;
            }

            if (!animator) return;
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(clip))
                animator.Play(clip);

            Debug.Log($"[{name}] plays '{clip}' for {action}");
        }
        #endregion

        // ────────────────────────────────────────────────
        #region Position Control
        public virtual void SetPosition(Vector3 newPosition, bool syncChild = true)
        {
            position = newPosition;
            transform.position = newPosition;

            if (syncChild && attachedTransform)
                attachedTransform.position = newPosition;
        }

        public virtual void SetRotation(Quaternion newRotation, bool syncChild = true)
        {
            rotation = newRotation;
            transform.rotation = newRotation;

            if (syncChild && attachedTransform)
                attachedTransform.rotation = newRotation;
        }

        public virtual void Teleport(Vector3 newPosition)
        {
            SetPosition(newPosition);
            Log($"[EntityBase] {this.gameObject.name + characterName} teleported to {newPosition}");
        }

        public virtual void MoveSmooth(Vector3 target, float speed = 5f)
        {
            position = Vector3.Lerp(position, target, Time.deltaTime * speed);
            transform.position = position;
            if (attachedTransform)
                attachedTransform.position = position;
        }
        #endregion

        // --- Combat ---
        public virtual void TakeDamage(int amount)
        {
            if (isDead || isInvulnerable || amount <= 0) return;

            stats.CurrentHealth -= amount;
            stats.CurrentHealth = Mathf.Max(stats.CurrentHealth, 0);


            if (stats.CurrentHealth == 0)
            {
                OnDeath();
            }
            else
            {
                PlayHitBlink();
                OnDamaged(amount);
            }
        }

        protected void PlayHitBlink()
        {
            if (blinkRoutine != null)
                StopCoroutine(blinkRoutine);

            blinkRoutine = StartCoroutine(Blink());
        }

        private IEnumerator Blink()
        {
            sprite.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration);
            sprite.color = originalColor;
        }

        public virtual void Heal(int amount)
        {
            if (isDead || amount <= 0) return;

            int oldHealth = stats.CurrentHealth;
            stats.CurrentHealth = Mathf.Min(stats.MaxHealth, stats.CurrentHealth + amount);
            int healedAmount = stats.CurrentHealth - oldHealth;

            if (healedAmount > 0)
            {
                OnHealedEvent?.Invoke(healedAmount);
                Log($"[EntityBase] {this.gameObject.name + characterName} healed by {healedAmount} HP");
            }
        }

        public virtual void AddExperience(int amount)
        {
            if (isDead || amount <= 0) return;

            stats.Experience += amount;
            int threshold = 100 * stats.Level;

            while (stats.Experience >= threshold)
            {
                stats.Experience -= threshold;
                LevelUp();
                threshold = 100 * stats.Level;
            }
        }

        public virtual void AddScore(int value)
        {
            score += Mathf.Max(0, value);
        }

        protected virtual void LevelUp()
        {
            stats.Level++;
            stats.MaxHealth += 10;
            stats.AttackPower += 2;
            stats.CurrentHealth = stats.MaxHealth;
            OnLevelUpEvent?.Invoke();
            Log($"[EntityBase] {this.gameObject.name + characterName} leveled up to {stats.Level}!");
        }

        // --- Events ---
        protected virtual void OnDamaged(int amount)
        {
            Log($"[EntityBase] {characterName} took {amount} damage ({stats.CurrentHealth}/{stats.MaxHealth})");
            IsRecentlyDamaged = true;
            OnDamagedEvent?.Invoke(amount);
        }

        public virtual void OnDeath()
        {
            if (isDead) return;

            isDead = true;
            stats.CurrentHealth = 0;

            Log($"[EntityBase] {this.gameObject.name + characterName} has been defeated!");
            OnDeathEvent?.Invoke();

            //if (attachedTransform)
            //    attachedTransform.gameObject.SetActive(false);

            //gameObject.SetActive(false);
            Log($"[EntityBase] {this.gameObject.name + characterName} died.");
        }

        public virtual void Die()
        {
            gameObject.SetActive(false);
        }

        // --- Utility ---
        protected void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log(message);
        }

        // --- Rotation Helper ---
        public void RotateTowards(Vector3 targetPosition, float rotationSpeed = 5f)
        {
            if (isDead) return;

            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                rotation = transform.rotation;
            }
        }

        public virtual EntityDataModel ToData()
        {
            return new EntityDataModel
            {
                Id = characterId,
                DisplayName = characterName,
                TeamId = teamId,
                Position = position,
                Rotation = rotation,
                AttachedTransform = attachedTransform,
                UseAttachedAsPrimary = useAttachedAsPrimary,
                Stats = stats,
                Score = 0,
                IsInvulnerable = isInvulnerable,
                IsDead = isDead,
                LocalizationKey = localizationKey,
                IsHaveSpellCast = isHaveSpellCast,
                IsTurn = isTurn,
            };
        }

        public virtual void FromData(EntityDataModel data)
        {
            characterId = data.Id;
            characterName = data.DisplayName;
            teamId = data.TeamId;
            position = data.Position;
            rotation = data.Rotation;
            attachedTransform = data.AttachedTransform;
            useAttachedAsPrimary = data.UseAttachedAsPrimary;
            stats = data.Stats;
            score = data.Score;
            isInvulnerable = data.IsInvulnerable;
            isDead = data.IsDead;
            localizationKey = data.LocalizationKey;
            isHaveSpellCast = data.IsHaveSpellCast;
            isTurn = data.IsTurn;
        }

        // --- Initialization from CharacterData ---
        public virtual void InitializeFromCharacter(CharacterData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[EntityBase] No CharacterData provided!");
                return;
            }

            characterName = data.CharacterName;
            characterDescription = data.CharacterDescription;
            characterType = data.CharacterType;
            characterId = data.CharacterId;
            teamId = data.TeamID;

            stats.MaxHealth = data.MaxHealth;
            stats.CurrentHealth = data.CurrentHealth;
            stats.AttackPower = data.AttackPower;
            stats.Defense = data.Defense;
            stats.MagicDefense = data.MagicDefense;
            stats.MagicPower = data.MagicPower;
            stats.Agility = data.Agility;
            stats.AttackRate = data.AttackRate;
            stats.AttackRange = data.AttackRange;
            stats.Mana = data.Mana;
            stats.Level = data.Level;
            stats.Experience = data.Experience;
            stats.CritChance = data.CritChance;
            stats.CritMultiplier = data.CritMultiplier;
            stats.InventoryData = data.InventoryData;

            isDead = false;
            gameObject.SetActive(true);

            Log($"[EntityBase] Initialized {this.gameObject.name + characterName} from CharacterData ({data.CharacterName})");
        }

        public virtual GameObject SpawnChild(string type)
        {
            Debug.LogWarning($"[{this.gameObject.name + characterName}] cannot spawn child of type '{type}'.");
            return null;
        }

        public virtual void PlayAnimation(string name)
        {
            Log($"[EntityBase] {this.gameObject.name + characterName} attempts to play animation: {name}.");
        }
        public virtual void PlayAnimation(string name, float speed)
        {
            Log($"[EntityBase] {this.gameObject.name + characterName} attempts to play animation {name} with speed : {speed}.");
        }
        public virtual void PlayAnimationInFixed(string name, float time)
        {
            Log($"[EntityBase] {this.gameObject.name + characterName} attempts to play animation {name} with time : {time}.");
        }

        public virtual void PlayAnimationInParameter(string parameterName, bool parameterValue)
        {
            Log($"[EntityBase] {this.gameObject.name + characterName} attempts to play direct parameter animation with param {parameterName} value {parameterValue}");
        }

        public virtual IEnumerator PlayAndWait(string parameterName, bool parameterValue)
        {
            yield return null;
            Log($"[EntityBase] {this.gameObject.name + characterName} attempts to play coroutine animation with param {parameterName} value {parameterValue}");
        }

        public virtual void ApplyMovement()
        {
            Log($"[EntityBase] {this.gameObject.name + characterName} apply move");
        }
        public virtual void Talk()
        {
            Log($"[EntityBase] {this.gameObject.name + characterName} has nothing to say.");
        }

        public virtual void SetStats(StatBlock newStats)
        {
            stats = newStats;
            Log($"[EntityBase] {this.gameObject.name + characterName} stats updated.");
        }

        public virtual void GetStats(out StatBlock outStats)
        {
            outStats = stats;
            Log($"[EntityBase] {this.gameObject.name + characterName} stats retrieved.");
        }
    }
}