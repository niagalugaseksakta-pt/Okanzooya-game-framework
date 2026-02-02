using Game.Model.Player;
using UnityEngine;

public class DpadJoystick : MonoBehaviour
{
    [Header("⚙️ Linked Entity")]
    public PlayerEntity playerEntity;

    public Vector2 Direction { get; private set; }
    private float directionMultiplier = 0.0035f;

    // =========================================================
    // 🧭 DPAD STATE
    // =========================================================
    private bool dpadActive;
    private Vector2 dpadDirection;

    // =========================================================
    // UNITY
    // =========================================================
    private void Awake()
    {
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }
    }

    private void FixedUpdate()
    {
        if (!playerEntity) return;

        // Only apply D-pad input while active. If not active, do not overwrite other input sources.
        if (!dpadActive)
        {
            Direction = Vector2.zero;
            return;
        }

        Vector2 value = dpadDirection;

        // Use clamped values for Horizontal and Vertical
        Vector2 clamped = new Vector2(
            Mathf.Clamp(value.x, -1f, 1f),
            Mathf.Clamp(value.y, -1f, 1f)
        );

        // Direction exposed uses the clamped values
        Direction = clamped;

        playerEntity.Horizontal = clamped.x;
        playerEntity.Vertical = clamped.y;

        if (!playerEntity.isInInteractions)
            playerEntity.ApplyMovement();
    }

    private void OnDisable()
    {
        ResetInput();
    }

    // =========================================================
    // 🧭 DPAD PUBLIC API (UNTUK UI BUTTON)
    // =========================================================
    public void DPadPressUp()
    {
        StartDPad(Vector2.up);
        Debug.Log("DPadPressUp");
    }
    public void DPadPressDown()
    {
        StartDPad(Vector2.down);
        Debug.Log("DPadPressDown");
    }
    public void DPadPressLeft()
    {
        StartDPad(Vector2.left);
        Debug.Log("DPadPressLeft");
    }
    public void DPadPressRight()
    {
        StartDPad(Vector2.right);
        Debug.Log("DPadPressRight");
    }

    public void DPadRelease()
    {
        ResetInput();
    }


    private void StartDPad(Vector2 dir)
    {
        dpadActive = true;
        dpadDirection = dir.normalized;
        Direction = dpadDirection;

        // Apply immediately so movement is responsive even before next FixedUpdate
        if (playerEntity != null)
        {
            playerEntity.Horizontal = dpadDirection.x;
            playerEntity.Vertical = dpadDirection.y;
            if (!playerEntity.isInInteractions)
                playerEntity.ApplyMovement();
        }

        Debug.Log($"dpadDirection {dpadDirection}");
    }

    private void ResetInput()
    {
        dpadActive = false;
        dpadDirection = Vector2.zero;
        Direction = Vector2.zero;

        if (playerEntity != null)
        {
            playerEntity.Horizontal = 0f;
            playerEntity.Vertical = 0f;
        }
    }
}
