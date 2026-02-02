using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game.Config.Config;

namespace Game.Model
{
    public class EntityProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float Lifetime;
        [Header("Throw Settings")]
        [SerializeField] private float throwSpeed;
        public int DamageMultiplier = 1;
        [SerializeField] protected int damage = 10;
        private EntityBase casterObject;
        [Header("Object Settings")]
        protected Transform casterContainer;
        protected Transform freeCasterContainer;
        protected EntityBase target;
        protected GameObject projectile;

        [Header("Animation Projectile Settings")]
        protected Dictionary<EntityProjectileActionType, string> animationMap = new();
        protected Animator animator;

        protected float timer;
        protected float GetLifeTime() => Lifetime;
        public Transform[] patrolPoints;
        public float moveSpeedPatrol;
        public float arriveThreshold;
        private int currentPatrolIndex = 0;
        public float fireCooldown;
        public float fireRate = 1f;
        private bool isCasted;

        private Vector2 freeThrowDirection;   // arah joystick dari luar
        private bool hasFreeDirection;        // apakah mode free-throw aktif

        private void Awake()
        {
            isCasted = true;
            RegisterDefaultAnimations();
        }


        #region "Animation Management"
        /// <summary>
        /// Each subclass overrides to define its own animation mapping.
        /// Smoke is template default.
        /// </summary>
        protected virtual void RegisterDefaultAnimations()
        {
            animationMap[EntityProjectileActionType.Pre] = "Smoke";
            animationMap[EntityProjectileActionType.Mid] = "Smoke";
            animationMap[EntityProjectileActionType.Finish] = "Smoke";
        }

        /// <summary>
        /// High-level call for animation through logical action type.
        /// </summary>
        public virtual void PlayAction(EntityProjectileActionType action)
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

        /// <summary>
        /// High-level call for animation through logical action type.
        /// </summary>
        public virtual void PlayAction(EntityProjectileActionType action, float speed)
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
            {
                animator.speed = speed;
                animator.Play(clip);
            }

            Debug.Log($"[{name}] plays '{clip}' for {action}");
        }


        // Build
        public virtual void Initialize(Transform from, EntityBase to, GameObject obj, Transform freeCast)
        {
            casterContainer = from;
            target = to;
            projectile = obj;
            freeCasterContainer = freeCast;
            StartCoroutine(FlyToTarget());
            //PlayAnimation("Fly");


        }

        public virtual void InitializeWithoutTarget(
    Transform from,
    GameObject obj,
    Transform freeCast,
    Vector2 joystickDirection)
        {
            casterContainer = from;
            projectile = obj;
            freeCasterContainer = freeCast;

            // normalize arah joystick
            freeThrowDirection = joystickDirection.normalized;
            hasFreeDirection = freeThrowDirection.sqrMagnitude > 0.1f;

            StartCoroutine(FlyToTargetWithJoystick());
        }


        // 
        protected virtual IEnumerator FlyToTarget()
        {
            isCasted = true;
            timer = 0f;

            if (target == null)
            {
                while (timer < Lifetime)
                {

                    // Add cooldown
                    fireCooldown -= Time.deltaTime;
                    if (fireCooldown <= 0f)
                    {
                        fireCooldown = fireRate;

                        Vector2 direction = (freeCasterContainer.transform.position - projectile.transform.position).normalized;

                        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                        if (rb != null)
                            rb.linearVelocity = direction * throwSpeed; // bullet speed

                    }

                    //Vector2 direction = ((Vector2)freeCasterContainer.transform.position - (Vector2)transform.position).normalized;
                    //Vector2 dir = (freeCasterContainer.transform.position - transform.position);

                    //Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                    //if (rb != null)
                    //    rb.MovePosition(rb.position + dir.normalized * throwSpeed * Time.fixedDeltaTime);

                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            while (target != null && !target.IsDead && timer < Lifetime)
            {
                //transform.position = Vector3.MoveTowards(
                //    transform.position,
                //    target.transform.position,
                //    Speed * Time.deltaTime
                //);

                // Give velocity directly
                // Direction based on caster facing (right vector)

                //Vector2 direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                fireCooldown -= Time.deltaTime;
                if (fireCooldown <= 0f)
                {
                    fireCooldown = fireRate;

                    Vector2 direction = (target.transform.position - projectile.transform.position).normalized;

                    Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.linearVelocity = direction * throwSpeed; // bullet speed

                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        protected virtual IEnumerator FlyToTargetWithJoystick()
        {
            isCasted = true;
            timer = 0f;

            if (target == null)
            {
                Vector2 direction = hasFreeDirection
                    ? freeThrowDirection                              // arah joystick
                    : (freeCasterContainer.position - transform.position).normalized;

                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

                while (timer < Lifetime)
                {
                    fireCooldown -= Time.deltaTime;
                    if (fireCooldown <= 0f)
                    {
                        fireCooldown = fireRate;

                        if (rb != null)
                            rb.linearVelocity = direction * throwSpeed;
                    }

                    timer += Time.deltaTime;
                    yield return null;
                }
            }

        }


        protected virtual void OnHitTarget()
        {
            StartCoroutine(DestroyAfterAnimation());
        }

        protected IEnumerator DestroyAfterAnimation()
        {
            yield return new WaitForSeconds(1f); //Handling animation phase
            Destroy(projectile);
            Destroy(gameObject);
            //isCasted = false;
        }

    }
}
