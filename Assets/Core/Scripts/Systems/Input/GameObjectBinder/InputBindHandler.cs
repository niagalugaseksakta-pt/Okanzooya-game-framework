using UnityEngine;
using UnityEngine.InputSystem;

public class InputBindHandler : MonoBehaviour
{
    [SerializeField] private RectTransform panel;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void OnBack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Back button pressed!");
            OnBack();
        }
    }

    public void OnBack()
    {
        Debug.Log("MenuPanel closing...");
        if (panel != null)
        {
            panel.gameObject.SetActive(false);
        }
    }
}
