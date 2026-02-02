using UnityEngine;
using UnityEngine.UI;
using static Game.Config.Config;

public class InputModeSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToggleGroup toggleGroup;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Listen to changes on all toggles
        foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    OnToggleSelected(toggle.name);
            });
        }
    }

    private void OnToggleSelected(string toggleName)
    {
        Debug.Log($"[InputModeSelector] Selected mode: {toggleName}");

        switch (toggleName)
        {
            case "Toggle_Keyboard":
                SetInputMode(InputMode.Keyboard);
                break;
            case "Toggle_Mouse":
                SetInputMode(InputMode.Mouse);
                break;
            case "Toggle_Joystick":
                SetInputMode(InputMode.Joystick);
                break;
            case "Toggle_Touchscreen":
                SetInputMode(InputMode.Touchscreen);
                break;
        }
    }

    private void SetInputMode(InputMode mode)
    {
        // Store in PlayerPrefs or a global manager
        PlayerPrefs.SetInt("InputMode", (int)mode);
        PlayerPrefs.Save();

        Debug.Log($"[InputModeSelector] Input mode saved as {mode}");
    }
}
