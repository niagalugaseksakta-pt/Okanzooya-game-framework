using UnityEngine;
using static Game.Config.Config;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private IInputHandler currentInput;
    public InputMode CurrentMode { get; private set; } = InputMode.Keyboard;

    [SerializeField] private JoystickController joystickRef;

    private TouchInput touchInput = new TouchInput();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetInputMode(CurrentMode);
    }

    void Update()
    {
        if (CurrentMode == InputMode.Touchscreen)
            touchInput.Update();
    }

    public void SetInputMode(InputMode mode)
    {
        CurrentMode = mode;
        switch (mode)
        {
            case InputMode.Keyboard:
                currentInput = new KeyboardInput();
                break;
            case InputMode.Joystick:
                currentInput = new JoystickInput(joystickRef);
                break;
            case InputMode.Mouse:
                currentInput = new MouseInput();
                break;
            case InputMode.Touchscreen:
                currentInput = touchInput;
                break;
        }

        Debug.Log($"Switched to {mode} input mode.");
    }

    public IInputHandler GetInput() => currentInput;
}
