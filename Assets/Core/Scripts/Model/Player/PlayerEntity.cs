using DG.Tweening;
using Game.Model.Data.Player;
using Game.Model.Struct;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Game.Config.Config;


namespace Game.Model.Player
{
    /// <summary>
    /// Extends EntityBase with advanced systems:
    /// - Dialogue interaction (talking)
    /// - Inventory management
    /// - Animation mapping
    /// - Child sprite attachment (e.g., bullets, drones, wings)
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerEntity : EntityBase
    {
        // === Can Do Or Have ===
        // == Some Script Partialy Implemented Manual Script

        // == emoticon system  ==
        [Header("Alert Emote (once only)")]
        [SerializeField] private Emoticon emoticon;
        private bool alertShown = false;

        // === TALK SYSTEM ===
        [Header("Talk System")]

        public bool isInInteractions;
        [SerializeField] private List<string> dialogueLines = new List<string>();
        private int currentDialogueIndex = 0;
        public event Action<string> OnSpeak;

        // === ANIMATION SYSTEM ===
        [Header("Animation States")]
        [Tooltip("By Clip Name")]
        [SerializeField] private AnimationClip[] animationStates;
        private string currentAnimation = "";
        private bool isAnimationLocked = false;

        // === CHILD SPRITES (BULLETS / DRONES / FLYERS) ===
        [Header("Child Entities")]
        [SerializeField] private Transform childContainer;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private List<GameObject> activeChildren = new();

        [Header("🏃 Player Reference")]
        public Rigidbody2D playerRigidBody;

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runningSpeed = 9f;



        private int jumpCount = 0;
        public bool isGrounded;
        public bool queuedAttack;      // queued click from ButtonB
        private bool queuedRunToggle;   // queued click from ButtonX
        private bool facingRight = true;
        // Expose facing so external systems (camera, ground check) can read it reliably
        public bool FacingRight => facingRight;
        // --- RUNTIME ---
        private float horizontal;
        private float vertical;
        [Header("🏃 Player Physics")]
        private Rigidbody2D entityRigidbody2D;
        private Vector2 originalColliderSize;
        private Vector2 originalColliderOffset;
        private bool crouchSetupDone = false;

        // ============================================================
        // 🔍 INTERNAL STATE (PRIVATE SET, PUBLIC GET)
        // ============================================================

        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        public bool IsIdle { get; private set; }
        public bool IsInMidAir { get; private set; }
        public bool IsInMidAirAttack { get; private set; }
        public bool IsWalk { get; private set; }
        public bool IsAttack { get; private set; }
        public bool IsWalkAttack { get; private set; }
        public bool IsButtonRunPress { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsRunAttack { get; private set; }
        public bool IsLanding { get; private set; }
        public bool IsHurt { get; private set; }
        public bool IsDie { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsLookUp { get; private set; }
        public bool IsLookDown { get; private set; }
        public bool IsButtonCrouchPress { get; private set; }
        public bool IsCrouch { get; private set; }
        public bool IsInWall { get; private set; }

        public float GetSpeed() => moveSpeed;

        // ============================================================
        // 🧩 METHOD SETTER — PENGGANTI FLAG PRIVATE
        // ============================================================
        // Pemanggilan dilakukan oleh controller (input/motion/anim)

        public void SetIdle(bool value) => IsIdle = value;
        public void SetInMidAir(bool value) => IsInMidAir = value;
        public void SetInMidAirAttack(bool value) => IsInMidAirAttack = value;
        public void SetWalk(bool value) => IsWalk = value;
        public void SetAttack(bool value) => IsAttack = value;
        public void SetWalkAttack(bool value) => IsWalkAttack = value;
        public void SetButtonRunPress(bool value) => IsButtonRunPress = value;
        public void SetRunning(bool value) => IsRunning = value;
        public void SetRunAttack(bool value) => IsRunAttack = value;
        public void SetLanding(bool value) => IsLanding = value;
        public void SetHurt(bool value) => IsHurt = value;
        public void SetDie(bool value) => IsDie = value;
        public void SetJumping(bool value) => IsJumping = value;
        public void SetLookUp(bool value) => IsLookUp = value;
        public void SetLookDown(bool value) => IsLookDown = value;
        public void SetButtonCrouchPress(bool value) => IsButtonCrouchPress = value;
        public void SetCrouch(bool value) => IsCrouch = value;
        public void SetInWall(bool value) => IsInWall = value;
        public void SetState(PlayerState newState)
        {
            CurrentState = newState;
        }

        [Header("🏃 Player Jump")]
        [SerializeField] private int maxJumps = 2;  // total allowed jumps (1 = normal, 2 = double)
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float doubleJumpForceMultiplier = 0.9f; // can tweak for softer second jump
        [SerializeField] private float wallSlideSpeed = 1.5f;
        [SerializeField] private float wallJumpForceX = 8f;
        [SerializeField] private float wallJumpForceY = 10f;

        [SerializeField] private float wallJumpGraceTime = 0.15f;
        [SerializeField] private float wallJumpGraceCounter;
        private bool wallWasTouched = false;
        public bool CanWallJump => wallJumpGraceCounter > 0f;

        public float Horizontal { get => horizontal; set => horizontal = value; }
        public float Vertical { get => vertical; set => vertical = value; }

        private bool isTouchingWall = false;
        private bool isWallSliding = false;
        private bool isWallJumping = false;
        private float wallJumpDirection;

        [Header("Layering Event Action Player Reference")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private LayerMask ceilingLayer;

        [Header("Dash method")]
        [SerializeField] private float dashForce = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashRange = 4f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private GameObject dashShadowPrefab; // optional: for ghost trail

        public int maxAirDash = 5;
        private int dashCount = 0;

        private Vector2 lastMoveDir = Vector2.right;

        private bool isDashing = false;
        private bool canDash = true;
        private Vector2 dashDirection;
        private Sequence dashSequence;


        [Header("Caster Settings")]
        public Transform casterContainer;
        public Transform casterContainerUltimate;
        public Transform freeCasterContainer;
        public GameObject spellPrefab;
        public GameObject spellPrefabUltimate;
        private GameObject objSpawn;
        private EntityProjectile attackObj;
        public float detectionRange = 100f;
        [SerializeField] private int targetTeamID = 2;
        private GameObject objSpawnUltimate;
        private EntityProjectile attackObjUltimate;
        public bool IsCastedSpell;
        private GameObject fx;
        private Vector2 joyDir;
        private bool IsGoingToGround;

        public event Func<bool> OnCastSpell;

        [Header("UI Button Action")]
        public UnityEvent onActionButton;

        private UnityEvent defaultAttackAction;

        [Header("Game Over Handler Items")]
        [SerializeField] private GameObject panelGameOver;
        [SerializeField] private GameObject panelButtonGameover;
        [SerializeField] private GameObject mainButtonPanel;
        [SerializeField] private GameObject statusUiPanel;
        [SerializeField] private GameObject[] joystick;

        [SerializeField] private AudioSource attackClip;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponentInChildren<Animator>();
            attackClip = GetComponent<AudioSource>();
            // Ensure we have a consistent Rigidbody2D reference.
            // Prefer inspector-assigned `playerRigidBody` if present; otherwise fall back to GetComponent.
            if (playerRigidBody == null)
            {
                entityRigidbody2D = GetComponent<Rigidbody2D>();
                playerRigidBody = entityRigidbody2D;
            }
            else
            {
                entityRigidbody2D = playerRigidBody;
            }

            // Diagnostic warning if body type or constraints would prevent horizontal movement.
            if (entityRigidbody2D != null)
            {
                if (entityRigidbody2D.bodyType != RigidbodyType2D.Dynamic)
                    Debug.LogWarning($"[PlayerEntity] Rigidbody2D bodyType is {entityRigidbody2D.bodyType}. It should be Dynamic for physics movement.", this);

                // If constraints are used, show them (cannot detect FreezePosition per-axis in 2D directly, but we warn anyway)
                if ((entityRigidbody2D.constraints & RigidbodyConstraints2D.FreezePositionX) != 0)
                    Debug.LogWarning("[PlayerEntity] Rigidbody2D has FreezePositionX constraint set. This will prevent horizontal movement.", this);
            }
            else
            {
                Debug.LogError("[PlayerEntity] No Rigidbody2D found on player. Movement will not work.", this);
            }

            fx = new GameObject();
            if (childContainer == null)
                childContainer = transform; // fallback
            defaultAttackAction = new UnityEvent();
            defaultAttackAction.AddListener(CastSpell);

            if (panelGameOver == null)
            {
                panelGameOver = FinderTagHelper.FindTagged("PanelGameOver");
            }

            if (panelButtonGameover == null)
            {
                panelButtonGameover = FinderTagHelper.FindTagged("PanelGameOverButton");
            }


            DontDestroyOnLoad(gameObject);
        }


        protected override void Start()
        {
            base.Start();

            try
            {

                //saveManager.DeleteSave();
                //saveManager.LoadInto(this);

                if (isDead)
                {
                    Debug.LogWarning("[PlayerEntity] Player was dead on load, respawning with minimal HP.");
                    stats.CurrentHealth = Mathf.Max(1, stats.MaxHealth / 2);
                    isDead = false;
                    gameObject.SetActive(true);
                }

                Debug.Log($"[PlayerEntity] Loaded saved data for {characterName} (HP: {stats.CurrentHealth}/{stats.MaxHealth})");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerEntity] No save found, initializing defaults. {ex.Message}");
            }
        }

        protected override void Update()
        {


            //ResetAllStateAttack();
            HandleMove();
            HandleQueuedInputs();

        }


        // ------------------------------
        // INTERACTION SWITCH
        // ------------------------------
        public void SetInteractionAction(UnityAction interaction)
        {
            onActionButton.RemoveAllListeners();
            onActionButton.AddListener(interaction);
        }

        public void RestoreAttackAction()
        {
            onActionButton.RemoveAllListeners();
            onActionButton.AddListener(CastSpell);
        }

        // ------------------------------
        // UI BUTTON CALLS THIS
        // ------------------------------
        public void OnActionButtonPressed()
        {
            onActionButton?.Invoke();
        }

        protected override void ShowAlertEmote(EmoteState emoteState)
        {
            StartCoroutine(ReleaseAlertAfterDelay(emoteState));
        }

        private IEnumerator ReleaseAlertAfterDelay(EmoteState emoteState)
        {
            alertShown = true;

            emoticon.gameObject.SetActive(true);
            emoticon.PlayEmote(emoteState);

            yield return new WaitForSeconds(1f); // delay 1 detik
            alertShown = false;
            emoticon.gameObject.SetActive(false);
        }


        public void HandleButtonPressInput()
        {
            if (IsButtonRunPress)
            {
                IsRunning = true;
                IsWalk = false;
            }
            else
            {
                IsRunning = false;
                IsWalk = true;
            }

            if (IsButtonCrouchPress)
            {
                IsCrouch = true;
            }
            else
            {
                IsCrouch = false;
            }
            Debug.Log($"IsRunning is {IsRunning}");
        }

        private void FixedUpdate()
        {
            HandleFalling();
            HandleWallJump();

            HandleButtonPressInput();
            if (!isInInteractions)
            {
                ApplyMovement();
            }
        }

        private void HandleFalling()
        {
            float vy = entityRigidbody2D.linearVelocity.y;

            // === CEK GROUNDING ===
            bool groundedNow = isGrounded;

            // === MASUK UDARA ===
            if (!groundedNow)
            {
                SetInMidAir(true);

                // Sedang jatuh, bukan naik
                if (vy < -0.1f)
                {
                    IsJumping = false;
                    IsLanding = false;
                }

                return;
            }
        }

        private void HandleWallJump()
        {
            // === Saat baru menyentuh wall (ON ENTER) ===
            if (isTouchingWall && !wallWasTouched)
            {
                wallWasTouched = true;
                wallJumpGraceCounter = wallJumpGraceTime;   // RESET SEKALI
            }
            // === Saat TIDAK menyentuh wall (EXIT) ===
            else if (!isTouchingWall)
            {
                wallWasTouched = false;

                if (wallJumpGraceCounter > 0)
                    wallJumpGraceCounter -= Time.deltaTime;
            }

            // === Prevent negative ===
            if (wallJumpGraceCounter < 0)
                wallJumpGraceCounter = 0;
        }


        private void ResetAllBooleanForAnimationStates()
        {
            IsIdle = false;
            IsInMidAir = false;
            IsInMidAirAttack = false;
            IsWalk = false;
            IsAttack = false;
            IsWalkAttack = false;
            IsRunning = false;
            IsRunAttack = false;
            IsLanding = false;
            IsHurt = false;
            IsDie = false;
            IsJumping = false;
            IsLookUp = false;
            IsLookDown = false;
            IsCrouch = false;
            IsInWall = false;

            //special state
            //isGrounded = false;
            //isWallSliding = false;
        }


        private void LateUpdate()
        {
            HandleAnimation();
        }


        private void HandleMove()
        {
            if (Mathf.Abs(Horizontal) < 0.05f)
            {
                IsIdle = true;
                IsWalk = false;
                IsRunning = false;
            }
            else
            {
                IsIdle = false;
            }

            if (!IsRunning && !IsIdle) IsWalk = !IsRunning;
        }

        protected override void RegisterDefaultAnimations()
        {
            animationMap[EntityActionType.Attack] = "Attack";
            animationMap[EntityActionType.Die] = "Die";
            animationMap[EntityActionType.Hurt] = "Hurt";
            animationMap[EntityActionType.Idle] = "Idle";
            animationMap[EntityActionType.Jump] = "Jump";
            animationMap[EntityActionType.Jump_Airbone] = "Jump_Airbone";
            animationMap[EntityActionType.Jump_Airbone_Attack] = "Jump_Airbone_Attack";
            animationMap[EntityActionType.Jump_Airbone_Land] = "Jump_Airbone_Land";
            animationMap[EntityActionType.Run] = "Run";
            animationMap[EntityActionType.Run_Attack] = "Run_Attack";
            animationMap[EntityActionType.Walk] = "Walk";
            animationMap[EntityActionType.Walk_Attack] = "Walk_Attack";
        }

        // === TALKING SYSTEM ===
        public override void Talk()
        {
            if (dialogueLines.Count == 0)
            {
                Debug.Log("[PlayerEntity] No dialogue assigned.");
                return;
            }

            string line = dialogueLines[currentDialogueIndex];
            OnSpeak?.Invoke(line);
            Debug.Log($"[Talk] {characterName}: {line}");

            currentDialogueIndex = (currentDialogueIndex + 1) % dialogueLines.Count;
        }

        // === ANIMATION SYSTEM ===
        public override void PlayAnimationInFixed(string name, float time)
        {
            if (animator == null || animationStates.Length == 0)
            {
                Debug.LogWarning("[PlayerEntity] No animator or animation states set.");
                return;
            }

            var clip = Array.Find(animationStates, a => a.name == name);
            if (clip != null)
            {
                animator.Play(clip.name);
                Debug.Log($"[Animation] {characterName} plays '{clip.name}'");
            }
            else
            {
                Debug.LogWarning($"[Animation] Clip '{name}' not found.");
            }
        }

        public override void PlayAnimation(string name)
        {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);

            if (animator == null || animationStates.Length == 0)
            {
                Debug.LogWarning("[PlayerEntity] No animator or animation states set.");
                return;
            }

            var clip = Array.Find(animationStates, a => a.name == name);
            if (clip != null)
            {
                animator.Play(name);
                Debug.Log($"[Animation] {characterName} plays '{clip.name}'");
            }
            else
            {
                Debug.LogWarning($"[Animation] Clip '{name}' not found.");
            }
        }

        public override void PlayAnimationInParameter(string parameterName, bool parameterValue)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                Debug.LogWarning($"[Animation] animationCoroutine '{animationCoroutine}' not found.");
            }

            if (animationStates.Length == 0)
            {
                Debug.LogWarning("[PlayerEntity] No animator or animation states set.");
                return;
            }

            animationCoroutine = StartCoroutine(PlayAndWait(parameterName, parameterValue));
            Debug.Log($"[Animation] animationCoroutine start {animationCoroutine}");

        }

        public override void PlayAnimation(string name, float speed)
        {
            if (animator == null || animationStates.Length == 0)
            {
                Debug.LogWarning("[PlayerEntity] No animator or animation states set.");
                return;
            }

            var clip = Array.Find(animationStates, a => a.name == name);
            if (clip != null)
            {
                animator.speed = speed;
                animator.Play(clip.name);
                Debug.Log($"[Animation] {characterName} plays '{clip.name}'");
            }
            else
            {
                Debug.LogWarning($"[Animation] Clip '{name}' not found.");
            }
        }

        public override IEnumerator PlayAndWait(string parameterName, bool parameterValue)
        {
            if (animator == null)
            {
                Debug.LogWarning("[Animation] PlayAndWait called but animator is null.");
                yield break;
            }

            animator.SetBool(parameterName, parameterValue);
            yield return null;

            float elapsed = 0f;
            const float maxWait = 2.0f; // seconds - safety timeout to prevent infinite loops

            // Wait until the animator is no longer in a transition and the current state's normalizedTime >=1
            while (elapsed < maxWait)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

                // If state has progressed past the first loop (animation finished) or animator not playing state, exit
                if (!animator.IsInTransition(0) && state.normalizedTime >= 1f)
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= maxWait)
            {
                Debug.LogWarning($"[Animation] PlayAndWait timed out waiting for parameter '{parameterName}' to finish.");
            }

            animationCoroutine = null;
        }

        // =========================================================
        // 🚶 Movement & Animation
        // =========================================================
        public override void ApplyMovement()
        {
            if (isInInteractions) return;
            // --- Joystick Direction (Untuk Lempar Proyektil) ---
            joyDir = new Vector2(Horizontal, Vertical);

            // Normalisasi bila ada input arah
            if (joyDir.sqrMagnitude > 0.01f)
                joyDir = joyDir.normalized;
            else
                joyDir = Vector2.zero;

            // Debug joyDir
            // Debug.Log($"joyDir: {joyDir}");


            Vector2 v = playerRigidBody.linearVelocity;

            Debug.Log($"IsRunning value in ApplyMovement: {IsRunning}");
            // --- Handle Horizontal Movement ---
            float targetSpeed = IsRunning ? runningSpeed : moveSpeed;
            v.x = Horizontal * targetSpeed;
            Debug.Log($"v.x value in ApplyMovement: {v.x}");
            // --- Crouch Movement Restriction ---
            if (IsCrouch)
                v.x *= 0.5f; // slower movement when crouched

            playerRigidBody.linearVelocity = v;

            // --- Flip Facing Direction ---
            if (Horizontal > 0.05f && !facingRight) Flip();
            else if (Horizontal < -0.05f && facingRight) Flip();

            // --- Look Up / Down ---
            if (Mathf.Abs(Vertical) > 0.1f)
            {
                if (Vertical > 0.1f)
                {
                    IsLookUp = true;
                    IsLookDown = false;
                }
                else if (Vertical < -0.1f)
                {
                    IsLookUp = false;
                    IsLookDown = true;
                }
            }
            else
            {
                IsLookUp = false;
                IsLookDown = false;
            }


        }

        private void ResizeForCrouch(bool crouching)
        {
            // Assuming a CapsuleCollider2D — adjust if using BoxCollider2D
            var col = GetComponent<CapsuleCollider2D>();
            if (col == null) return;

            if (!crouchSetupDone)
            {
                originalColliderSize = col.size;
                originalColliderOffset = col.offset;
                crouchSetupDone = true;
            }

            if (crouching)
            {
                col.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.6f);
                col.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y * 0.2f);
            }
            else
            {
                col.size = originalColliderSize;
                col.offset = originalColliderOffset;
            }
        }

        // player inner function

        public void ButtonDownLongPressRun()
        {
            IsButtonRunPress = true;
        }
        public void ButtonUpLongPressRun()
        {
            IsButtonRunPress = false;

        }


        public void ButtonDownLongPressCrouch()
        {
            IsButtonCrouchPress = true;
        }
        public void ButtonUpLongPressCrouch()
        {
            IsButtonCrouchPress = false;

        }

        public void HandleQueuedInputs()
        {

            if (attackObj != null && IsCastedSpell)
            {

                HandleAttackLogic();

            }



        }


        public void HandleAttackLogic()
        {

            // Determine active attack based on movement state
            if (IsJumping && !isGrounded && IsCastedSpell)
            {
                animator.ResetTrigger("IsMidAirAttack");
                animator.SetTrigger("IsMidAirAttack");
            }
            else if (IsRunning && IsCastedSpell)
            {
                animator.ResetTrigger("IsRunAttack");
                animator.SetTrigger("IsRunAttack");
            }
            else if (IsWalk && IsCastedSpell)
            {
                animator.ResetTrigger("IsWalkAttack");
                animator.SetTrigger("IsWalkAttack");
            }
            else if (IsIdle && IsCastedSpell)
            {
                animator.ResetTrigger("IsAttack");
                animator.SetTrigger("IsAttack");
            }

            IsCastedSpell = !IsCastedSpell;

        }

        public void HandleAttackUltimate()
        {
            // Todo Special move later (-_-)
        }

        public void TryJump()
        {
            IsJumping = true;
            IsInMidAir = true;
            IsLanding = false;



            // =============================================
            // WALL JUMP
            // =============================================
            if (isTouchingWall && isWallSliding && CanWallJump && !isGrounded)
            {
                Vector2 jumpDir = Vector2.up;

                if (Mathf.Abs(Horizontal) > 0.1f)
                    jumpDir.x = Horizontal * 0.5f;

                jumpDir.Normalize();
                entityRigidbody2D.linearVelocity = jumpDir * wallJumpForceY;

                jumpCount = 0;          // reset jump
                dashCount = 0;          // reset dash
                isWallJumping = true;
                canDash = false;
                StartCoroutine(StopWallJump());
                return;
            }

            // =============================================
            // NORMAL / DOUBLE / MULTI-JUMP
            // =============================================
            if (jumpCount < maxJumps)
            {
                DoJumpAction();
                jumpCount++;
                Debug.Log($"Jump Count = {jumpCount}");

                // Tracking arah terakhir
                if (Mathf.Abs(Horizontal) > 0.1f || Mathf.Abs(Vertical) > 0.1f)
                    lastMoveDir = new Vector2(Horizontal, Vertical).normalized;

                return;
            }

            // =============================================
            // DASH HANYA SETELAH MAX JUMP TERCAPAI
            // =============================================
            if (!isGrounded && jumpCount >= maxJumps)
            {
                canDash = true;
                TryDashAfterJumpLimit();
            }
        }

        private void TryDashAfterJumpLimit()
        {
            if (dashCount < maxAirDash && canDash)
            {
                dashCount++;
                Debug.Log($"Dash Count = {dashCount}");
                StartCoroutine(PerformDash());
            }
        }



        private void DoJumpAction()
        {
            Vector2 v = entityRigidbody2D.linearVelocity;
            v.y = 0f;
            entityRigidbody2D.linearVelocity = v;

            float finalForce = jumpForce * (jumpCount == 0 ? 1f : doubleJumpForceMultiplier);
            entityRigidbody2D.linearVelocity += Vector2.up * finalForce;

        }

        private IEnumerator PerformDash()
        {
            if (isDashing || !canDash)
            {
                jumpCount = 0;
                yield break;
            }

            isDashing = true;

            float dashTime = 0.15f;
            float elapsed = 0f;

            float originalGravity = entityRigidbody2D.gravityScale;

            // ===== DETERMINE DASH DIRECTION =====
            //if (isTouchingWall)
            //{
            dashDirection = facingRight ? Vector2.right : Vector2.left;
            //}
            //else
            //{
            //    dashDirection = new Vector2(Horizontal, Vertical);

            //    if (dashDirection.magnitude < 0.1f)
            //        dashDirection = lastMoveDir;

            //    if (dashDirection.magnitude < 0.1f)
            //        dashDirection = Vector2.right;

            //    dashDirection.Normalize();
            //}

            // Compute speed
            float dashSpeed = dashRange / dashTime;

            // ===== PREPARE PHYSICS =====
            entityRigidbody2D.gravityScale = 0f;
            entityRigidbody2D.linearVelocity = Vector2.zero;

            animator?.SetTrigger("Dash");

            // Contact filter for wall checking
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(wallLayer);
            contactFilter.useTriggers = false;

            RaycastHit2D[] hitBuffer = new RaycastHit2D[4];

            // ===== DASH LOOP =====
            while (elapsed < dashTime)
            {
                float stepDt = Time.fixedDeltaTime;
                float moveDist = dashSpeed * stepDt;

                int hitCount = entityRigidbody2D.Cast(
                    dashDirection, contactFilter, hitBuffer, moveDist + 0.01f
                );

                if (hitCount > 0)
                {
                    RaycastHit2D hit = hitBuffer[0];
                    for (int i = 1; i < hitCount; i++)
                        if (hitBuffer[i].distance < hit.distance)
                            hit = hitBuffer[i];

                    Vector2 safePos = (Vector2)transform.position +
                                       dashDirection * Mathf.Max(0f, hit.distance - 0.01f);

                    entityRigidbody2D.MovePosition(safePos);
                    entityRigidbody2D.linearVelocity = Vector2.zero;

                    Vector2 reboundDir = hit.normal;
                    if (Vector2.Dot(reboundDir, -dashDirection) < 0.3f)
                        reboundDir = -dashDirection;

                    Vector2 rebound = (reboundDir + Vector2.up * 0.25f).normalized * (dashForce * 0.6f);
                    entityRigidbody2D.AddForce(rebound, ForceMode2D.Impulse);

                    animator?.SetTrigger("WallImpact");

                    entityRigidbody2D.gravityScale = originalGravity;
                    isDashing = false;

                    yield return new WaitForSeconds(0.08f);
                    yield break;
                }

                entityRigidbody2D.MovePosition((Vector2)transform.position + dashDirection * moveDist);

                elapsed += stepDt;
                yield return new WaitForFixedUpdate();
            }

            // ===== NORMAL EXIT =====
            entityRigidbody2D.gravityScale = originalGravity;
            entityRigidbody2D.linearVelocity = dashDirection * dashForce;

            isDashing = false;

            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }


        private IEnumerator StopWallJump()
        {
            yield return new WaitForSeconds(0.15f);
            isTouchingWall = false;
            isWallSliding = false;
            isWallJumping = false;
        }


        public void HandleAnimation()
        {
            if (animator == null) return;

            ResetAnimatorParameter();

            if (IsDie) PlayAnimationInParameter("IsDie", IsDie);
            if (IsHurt) PlayAnimationInParameter("IsHurt", IsHurt);

            if (!isGrounded)
            {
                if (IsJumping)
                {
                    animator?.SetTrigger(jumpCount < maxJumps ? "Jump" : "DoubleJump");
                    animator.SetBool("IsJump", IsJumping);
                    animator.SetBool("IsInMidAir", IsInMidAir);
                    animator.SetBool("IsLanding", IsLanding);
                }
                if (IsInMidAir)
                {
                    animator.SetBool("IsInMidAir", IsInMidAir);
                }
                //if (IsInMidAirAttack)
                //{
                //    animator.SetBool("IsInMidAir", IsInMidAir);
                //    animator.SetBool("IsInMidAirAttack", IsInMidAirAttack);
                //}

                if (IsInWall && isTouchingWall && isWallSliding && entityRigidbody2D.linearVelocity.y < 0)
                {
                    // entityRigidbody2D.linearVelocity = new Vector2(entityRigidbody2D.linearVelocity.x, -wallSlideSpeed);
                    animator?.SetTrigger("WallJump");
                    animator?.SetBool("IsWallSliding", isWallSliding);
                    animator?.SetBool("IsFacingWall", IsInWall);
                }

            }
            else
            {
                if (IsLanding)
                {
                    animator.SetBool("IsJump", IsJumping);
                    animator.SetBool("IsLanding", IsLanding);
                }
                if (IsRunning)
                {
                    animator.SetBool("IsRunning", IsRunning);
                    SetState(PlayerState.Run);
                }
                //if (IsRunning && IsRunAttack)
                //{
                //    animator.SetBool("IsRunning", IsRunning);
                //    animator.SetBool("IsRunAttack", IsRunAttack);
                //}

                if (IsWalk)
                {
                    animator.SetBool("IsWalk", IsWalk);
                    SetState(PlayerState.Walk);
                }
                //if (IsWalk && IsWalkAttack)
                //{
                //    animator.SetBool("IsWalk", IsWalk);
                //    animator.SetBool("IsWalkAttack", IsWalkAttack);
                //}

                if (IsIdle)
                {
                    animator.SetBool("IsIdle", IsIdle);
                    SetState(PlayerState.Idle);
                    //ShowAlertEmote();
                }
                //if (IsIdle && IsAttack)
                //{
                //    animator.SetBool("IsIdle", IsIdle);
                //}
                // --- Handle Crouch ---
                if (IsCrouch)
                {
                    // Set crouch animation
                    animator?.SetBool("IsCrouch", true);

                    // Resize collider or rigidbody shape
                    ResizeForCrouch(true);
                }
                else
                {
                    animator?.SetBool("IsCrouch", false);
                    ResizeForCrouch(false);
                }

                if (IsLookUp)
                {
                    animator?.SetBool("IsLookUp", true);
                    animator?.SetBool("IsLookDown", false);
                }
                else if (IsLookDown)
                {
                    animator?.SetBool("IsLookUp", false);
                    animator?.SetBool("IsLookDown", true);
                }
                else
                {
                    animator?.SetBool("IsLookUp", false);
                    animator?.SetBool("IsLookDown", false);
                }
            }

        }

        public void ResetAnimatorParameter()
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsInMidAir", false);
            animator.SetBool("IsInMidAirAttack", false);
            animator.SetBool("IsWalk", false);
            animator.SetBool("IsAttack", false);
            animator.SetBool("IsWalkAttack", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsRunAttack", false);
            animator.SetBool("IsLanding", false);
            animator.SetBool("IsHurt", false);
            animator.SetBool("IsDie", false);
            animator.SetBool("IsJump", false);
            animator.SetBool("IsLookUp", false);
            animator.SetBool("IsLookDown", false);
            animator.SetBool("IsCrouch", false);
            animator?.SetBool("IsWallSliding", false);
            animator?.SetBool("IsFacingWall", false);

            animator?.ResetTrigger("WallJump");
            animator?.ResetTrigger("Jump");
            animator?.ResetTrigger("DoubleJump");
            animator?.ResetTrigger("Dash");
        }

        public void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = playerRigidBody.transform.localScale;
            scale.x *= -1;
            playerRigidBody.transform.localScale = scale;
        }

        // === CHILD OBJECT SYSTEM ===
        public override GameObject SpawnChild(string type)
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("[PlayerEntity] No bullet prefab assigned.");
                return null;
            }

            GameObject obj = Instantiate(bulletPrefab, childContainer);
            obj.name = $"{characterName}_{type}_{activeChildren.Count}";
            activeChildren.Add(obj);

            Debug.Log($"[ChildSpawn] Spawned {obj.name} as child of {characterName}");
            return obj;
        }

        // === DEATH HANDLING ===
        public override void Die()
        {
            if (!isDead) return;

            base.Die();
            Debug.Log("[PlayerEntity] Player defeated! Respawning...");
            //saveManager?.Save(this);
            Debug.Log("[PlayerEntity] Data saved ....");
            PlayAnimation("Die");
            Invoke(nameof(Respawn), 2f);
        }

        private void Respawn()
        {
            isDead = false;
            stats.CurrentHealth = stats.MaxHealth;
            gameObject.SetActive(true);
            //saveManager?.Save(this);
            Debug.Log("[PlayerEntity] Respawn complete and saved!");
        }

        public override EntityDataModel ToData()
        {
            var baseData = base.ToData();
            baseData.DisplayName = "Player";
            return baseData;
        }

        public void LoadStatsFromCharacter(CharacterData character)
        {
            InitializeFromCharacter(character);
        }

        public override void SetStats(StatBlock newStats)
        {
            base.SetStats(newStats);
        }

        public override void GetStats(out StatBlock outStats)
        {
            base.GetStats(out outStats);
        }

        public bool IsPlaying(string name)
        {
            if (animator == null || animationStates.Length == 0)
            {
                Debug.LogWarning("[PlayerEntity] No animator or animation states set.");
                return false;
            }

            var state = animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName(name);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            int layer = collision.gameObject.layer;
            if (collision.contactCount == 0) return;

            if (collision.collider.CompareTag("Obstacle")) //force down check
            {
                IsInMidAir = false;
                IsLanding = true;
                isGrounded = true;
                IsJumping = false;
                jumpCount = 0;
                dashCount = 0;
            }

            if (((1 << layer) & groundLayer) != 0 && collision.contacts[0].normal.y > 0.5f) //force down check
            {
                IsInMidAir = false;
                IsLanding = true;
                isGrounded = true;
                IsJumping = false;
                jumpCount = 0;
                dashCount = 0;
            }

            if (collision.collider.CompareTag("Enemy"))
            {
                PlayAnimation("Hurt");
            }

            // Detect wall contact
            if (((1 << layer) & wallLayer) != 0 && Mathf.Abs(collision.contacts[0].normal.x) > 0.5f) // force side check
            {
                IsInWall = true;
                isTouchingWall = true;
                isWallSliding = true;
                wallJumpDirection = facingRight ? -1 : 1;
                jumpCount = 1;
                dashCount = 0;
            }

            if (((1 << layer) & ceilingLayer) != 0 && collision.contacts[0].normal.y < -0.5f)
            {

                // maybe cancel jump or adjust head bump
                //Debug.Log("Ceiling Contact Detected by Normal " + normal);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {

            int layer = collision.gameObject.layer;

            if (collision.collider.CompareTag("Obstacle")) //force down check
            {
                isGrounded = false;
                isWallSliding = false;
            }

            if (((1 << layer) & groundLayer) != 0) //force down check
            {
                isGrounded = false;
                isWallSliding = false;
            }

            if (collision.collider.CompareTag("Enemy"))
            {
                PlayAnimation("Hurt");
            }

            // Detect wall contact
            if (((1 << layer) & wallLayer) != 0) // force side check
            {
                IsJumping = false;
                IsInWall = false;
                isTouchingWall = false;
                isWallSliding = false;
            }

            if (((1 << layer) & ceilingLayer) != 0)
            {

                // maybe cancel jump or adjust head bump
                //Debug.Log("Ceiling Contact Detected by Normal " + normal);
            }
        }


        // Region cast projectile
        EntityBase FindTargetByTeam(int teamId)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(casterContainer.transform.position, detectionRange);
            float minDist = Mathf.Infinity;
            EntityBase closest = null;

            foreach (var hit in hits)
            {
                EntityBase e = hit.GetComponent<EntityBase>();
                if (e != null && e.TeamID == teamId && !e.IsDead)
                {
                    float dist = Vector2.Distance(casterContainer.transform.position, e.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = e;
                    }
                }
            }
            return closest;
        }

        // Prepare Init
        public void CastSpell()
        {
            if (spellPrefab == null || casterContainer == null)
                return;
            IsCastedSpell = true;

            //EntityBase target = FindTargetByTeam(targetTeamID);
            //if (target == null) return;
            // Create projectile
            objSpawn = Instantiate(spellPrefab, casterContainer.transform.position, casterContainer.transform.rotation);
            // Initialize projectile data
            attackObj = objSpawn.GetComponent<EntityProjectile>();
            if (attackObj != null)
            {
                attackObj.InitializeWithoutTarget(casterContainer, objSpawn, freeCasterContainer, joyDir);

            }

            attackClip.Play();

        }

        public void CastSpellUltimate()
        {
            if (spellPrefabUltimate == null || casterContainerUltimate == null)
                return;

            EntityBase target = FindTargetByTeam(targetTeamID);
            //if (target == null) return;
            // Create projectile
            objSpawnUltimate = Instantiate(spellPrefab, casterContainerUltimate.transform.position, casterContainerUltimate.transform.rotation);
            // Initialize projectile data
            attackObjUltimate = objSpawnUltimate.GetComponent<EntityProjectile>();
            if (attackObjUltimate != null)
            {
                attackObjUltimate.Initialize(casterContainerUltimate, target, objSpawnUltimate, freeCasterContainer);

            }
        }

        public override void OnDeath()
        {
            base.OnDeath();
            panelGameOver.SetActive(true);
            panelButtonGameover.SetActive(true);
            //mainButtonPanel.gameObject.GetComponent<Renderer>().enabled = false;
            //statusUiPanel.gameObject.GetComponent<Renderer>().enabled = false;
            foreach (var item in joystick)
            {
                item.gameObject.GetComponent<Image>().enabled = false;
            }

        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(casterContainer.transform.position, detectionRange);

        }
    }
}