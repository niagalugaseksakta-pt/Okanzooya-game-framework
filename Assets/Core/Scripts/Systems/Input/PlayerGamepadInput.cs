using Game.Model.Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGamepadInput : MonoBehaviour
{
    [Header("🎮 Linked Player")]
    public PlayerEntity playerEntity;

    [Header("🎮 Input Actions")]
    public InputActionReference moveAction;     // Vector2 (left stick)
    public InputActionReference jumpAction;     // Cross
    public InputActionReference attackAction;   // Square
    public InputActionReference runAction;      // L1 / R1 optional
    public InputActionReference crouchAction;
    public InputActionReference specialAction;  // Triangle (optional)

    private void Awake()
    {
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        moveAction?.action?.Enable();
        jumpAction?.action?.Enable();
        attackAction?.action?.Enable();
        runAction?.action?.Enable();
        crouchAction?.action?.Enable();
        specialAction?.action?.Enable();

        // Bind callbacks
        if (jumpAction)
            jumpAction.action.performed += _ => playerEntity?.TryJump();

        if (attackAction)
            attackAction.action.performed += _ => playerEntity?.CastSpell();

        if (runAction)
            runAction.action.started += _ => playerEntity?.ButtonDownLongPressRun();
        if (runAction)
            runAction.action.canceled += _ => playerEntity?.ButtonUpLongPressRun();

        if (crouchAction)
            crouchAction.action.performed += _ => playerEntity?.ButtonDownLongPressCrouch();
        if (crouchAction)
            crouchAction.action.canceled += _ => playerEntity?.ButtonUpLongPressCrouch();

        if (specialAction)
            specialAction.action.performed += _ => playerEntity?.CastSpellUltimate();
    }

    private void OnDisable()
    {
        moveAction?.action?.Disable();
        jumpAction?.action?.Disable();
        attackAction?.action?.Disable();
        runAction?.action?.Disable();
        crouchAction?.action?.Disable();
        specialAction?.action?.Disable();
    }

    private void FixedUpdate()
    {
        if (playerEntity == null) return;

        // Read left stick from PS4/PS5/Keyboard by input setup in Input Actions
        Vector2 move = moveAction ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        // Deadzone manual (optional)
        if (move.magnitude < 0.15f)
            move = Vector2.zero;

        // Map to player entity
        playerEntity.Horizontal = move.x;
        playerEntity.Vertical = move.y;

    }
}
