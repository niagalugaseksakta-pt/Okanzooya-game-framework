using Game.Model;
using System;
using UnityEngine;
using static Game.Config.Config;

[DisallowMultipleComponent]
public abstract class EntityObstacleBase : MonoBehaviour
{
    [Header("Obstacle Identity")]
    [SerializeField] private string obstacleId = Guid.NewGuid().ToString();
    [SerializeField] private string obstacleName = "Obstacle";
    [SerializeField, TextArea] private string obstacleDescription;

    [Header("Obstacle Type")]
    public ObstacleType obstacleType = ObstacleType.Static;

    [Header("Health / Destruction")]
    public bool destructible = false;
    public int maxHealth = 1;
    public int currentHealth = 1;

    [Header("Damage Behaviour")]
    public bool dealDamage = false;
    public int damageAmount = 1;

    [Tooltip("Jika true → damage hanya diberikan sekali saat menyentuh")]
    public bool damageOnce = false;

    [Tooltip("Cooldown antar damage jika tidak once")]
    public float damageCooldown = 0.5f;

    private bool hasDealtDamage = false;
    private float lastDamageTime = -999f;

    [Header("Effects")]
    public GameObject destroyEffect;

    public string ObstacleId => obstacleId;
    public string ObstacleName => obstacleName;
    public bool IsDestroyed => currentHealth <= 0;

    // -------------------------------------------------------------------

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(obstacleId))
            obstacleId = Guid.NewGuid().ToString();

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnInitialized();
    }

    protected virtual void Start() => OnStarted();
    protected virtual void Update() => OnUpdated();

    // -------------------------------------------------------------------
    //  Virtual Hooks (Override in children)
    // -------------------------------------------------------------------
    protected virtual void OnInitialized() { }
    protected virtual void OnStarted() { }
    protected virtual void OnUpdated() { }

    // -------------------------------------------------------------------
    //  Damage Handling for the Obstacle
    // -------------------------------------------------------------------
    public virtual void TakeDamage(int amount)
    {
        if (!destructible) return;
        if (IsDestroyed) return;

        currentHealth -= Mathf.Max(1, amount);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDestroyed();
        }
    }

    protected virtual void OnDestroyed()
    {
        if (destroyEffect)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------
    //  COLLISION → SEARCH EntityBase → APPLY DAMAGE
    // -------------------------------------------------------------------

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        HandleDamageCollision(col);
    }

    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        HandleDamageCollision(col.collider);
    }

    internal void HandleDamageCollision(Collider2D col)
    {
        if (!dealDamage) return;

        // Cooldown logic
        if (!damageOnce && Time.time - lastDamageTime < damageCooldown)
            return;

        // Cek apakah target memiliki EntityBase
        var entity = col.GetComponent<EntityBase>();
        if (entity == null) return;

        // Terapkan damage ke EntityBase
        entity.TakeDamage(damageAmount);

        lastDamageTime = Time.time;

        if (damageOnce)
            hasDealtDamage = true;
    }
}
