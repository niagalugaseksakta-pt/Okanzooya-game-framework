using UnityEngine;
using UnityEngine.UI;
using static Game.Config.Config;

public class OptionsMenu : MonoBehaviour
{
    public Dropdown inputDropdown;

    void Start()
    {
        inputDropdown.onValueChanged.AddListener(OnChangeInputMode);
    }

    private void OnChangeInputMode(int index)
    {
        InputManager.Instance.SetInputMode((InputMode)index);
    }
}
