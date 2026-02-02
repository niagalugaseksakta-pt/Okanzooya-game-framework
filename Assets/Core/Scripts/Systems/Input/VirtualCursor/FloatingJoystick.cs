using Game.Model.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using static Game.Config.Config;

[RequireComponent(typeof(RectTransform))]
public class FloatingJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public enum JoystickMode { Floating, Fixed }

    // =========================================================
    // 🎮 CONFIG
    // =========================================================
    [Header("🎮 UI Mode")]
    [SerializeField] private AxisMode axisMode = AxisMode.FreeXY;

    [Header("🎮 UI References")]
    [SerializeField] private RectTransform handle;

    [Header("🕹 Settings")]
    [SerializeField] private JoystickMode mode = JoystickMode.Floating;
    [Range(0f, 1f)] public float deadZone = 0.15f;
    public float radius = 120f;

    [Header("⚙️ Linked Entity")]
    public PlayerEntity playerEntity;

    [Header("📌 Screen Limit Area (Optional)")]
    public Rect screenLimit = new Rect(0, 0, 1920, 1080);
    public bool useScreenLimit = false;

    [Header("🚀 Drag Speed")]
    [SerializeField] private float dragSpeedMultiplier = 0.0035f; // tune this
    [SerializeField] private float maxSpeedBoost = 1.5f;

    private Vector2 lastPointerLocal;
    private float lastDragTime;
    private float dragSpeed; // pixels per second


    // =========================================================
    // 🔧 INTERNAL STATE (DEVICE-LIKE)
    // =========================================================
    private Canvas canvas;
    private Camera mainCamera;

    private bool pointerActive;
    private Vector2 startLocalPos;
    private Vector2 currentPointerLocal;

    public Vector2 Direction { get; private set; }

    // =========================================================
    // UNITY
    // =========================================================
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        mainCamera = canvas && canvas.worldCamera
            ? canvas.worldCamera
            : Camera.main;

        if (!handle)
            handle = GetComponent<RectTransform>();

        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }

        DontDestroyOnLoad(gameObject);
    }

    private void FixedUpdate()
    {
        if (!playerEntity)
            return;

        Vector2 value = ReadJoystickValue();

        Direction = value;

        playerEntity.Horizontal = Mathf.Clamp(value.x, -1f, 1f);
        playerEntity.Vertical = Mathf.Clamp(value.y, -1f, 1f);
        if (!playerEntity.isInInteractions)
        {
            playerEntity.ApplyMovement();
        }

    }

    private void OnDisable()
    {
        // Ensure we clear input state when joystick is disabled (prevent stale non-zero values)
        pointerActive = false;
        Direction = Vector2.zero;
        if (handle != null) handle.anchoredPosition = Vector2.zero;

        if (playerEntity != null)
        {
            playerEntity.Horizontal = 0f;
            playerEntity.Vertical = 0f;
        }
    }

    // =========================================================
    // POINTER EVENTS (DEVICE UPDATE ONLY)
    // =========================================================
    public void OnPointerDown(PointerEventData eventData)
    {
        if (useScreenLimit && !screenLimit.Contains(eventData.position))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            mainCamera,
            out startLocalPos
        );

        currentPointerLocal = startLocalPos;
        lastPointerLocal = startLocalPos;
        lastDragTime = Time.unscaledTime;

        pointerActive = true;

        if (handle != null) handle.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!pointerActive) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            mainCamera,
            out currentPointerLocal
        );

        float now = Time.unscaledTime;
        float deltaTime = now - lastDragTime;

        if (deltaTime > 0f)
        {
            float distance = Vector2.Distance(currentPointerLocal, lastPointerLocal);
            dragSpeed = distance / deltaTime; // pixels/sec
        }

        lastPointerLocal = currentPointerLocal;
        lastDragTime = now;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        pointerActive = false;
        Direction = Vector2.zero;
        if (handle != null) handle.anchoredPosition = Vector2.zero;

        if (playerEntity)
        {
            playerEntity.Horizontal = 0f;
            playerEntity.Vertical = 0f;
        }
    }

    // =========================================================
    // 🎮 INPUT DEVICE LOGIC (LIKE InputAction)
    // =========================================================
    private Vector2 ReadJoystickValue()
    {
        if (!pointerActive)
            return Vector2.zero;

        Vector2 rawDelta = currentPointerLocal - startLocalPos;

        // Clamp to radius (pixel space)
        Vector2 clamped = Vector2.ClampMagnitude(rawDelta, radius);

        // Visual
        if (handle != null) handle.anchoredPosition = clamped;

        // Normalized magnitude (0..1)
        float magnitude = clamped.magnitude / radius;

        if (magnitude < deadZone)
            return Vector2.zero;

        // Direction normalized
        Vector2 direction = clamped.normalized;

        // Speed factor (1 = normal, >1 = faster drag)
        float speedFactor = 1f + (dragSpeed * dragSpeedMultiplier);
        speedFactor = Mathf.Clamp(speedFactor, 1f, maxSpeedBoost);

        // Apply axis rules
        direction = ApplyAxisMode(direction);

        // Final value (-1..1) WITH SPEED
        return direction * Mathf.Clamp01(magnitude * speedFactor);

    }


    // =========================================================
    // AXIS MODES
    // =========================================================
    private Vector2 ApplyAxisMode(Vector2 d)
    {
        switch (axisMode)
        {
            case AxisMode.HorizontalOnly:
                return new Vector2(d.x, 0);

            case AxisMode.VerticalOnly:
                return new Vector2(0, d.y);

            case AxisMode.HorizontalAndVerticalOnly:
                return GetHorizontalAndVerticalDirection(d);

            case AxisMode.FourCorner:
                return GetFourCornerDirection(d);

            case AxisMode.EightDirection:
                return GetEightDirection(d);

            default:
                return d;
        }
    }

    private Vector2 GetHorizontalAndVerticalDirection(Vector2 d)
    {
        if (d == Vector2.zero) return Vector2.zero;

        return Mathf.Abs(d.x) > Mathf.Abs(d.y)
            ? new Vector2(d.x, 0f)
            : new Vector2(0f, d.y);
    }

    private Vector2 GetFourCornerDirection(Vector2 d)
    {
        if (d == Vector2.zero) return Vector2.zero;

        float mag = d.magnitude;
        float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        Vector2 dir =
            (angle >= 315 || angle < 45) ? new Vector2(1, 1) :
            (angle >= 45 && angle < 135) ? new Vector2(-1, 1) :
            (angle >= 135 && angle < 225) ? new Vector2(-1, -1) :
                                             new Vector2(1, -1);

        return dir.normalized * mag;
    }

    private Vector2 GetEightDirection(Vector2 d)
    {
        if (d == Vector2.zero) return Vector2.zero;

        float mag = d.magnitude;
        float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        int slice = Mathf.FloorToInt(angle / 45f);
        Vector2 dir = slice switch
        {
            0 => Vector2.right,
            1 => new Vector2(1, 1),
            2 => Vector2.up,
            3 => new Vector2(-1, 1),
            4 => Vector2.left,
            5 => new Vector2(-1, -1),
            6 => Vector2.down,
            7 => new Vector2(1, -1),
            _ => Vector2.zero
        };

        return dir.normalized * mag;
    }
}
