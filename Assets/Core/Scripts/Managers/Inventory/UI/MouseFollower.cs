using Inventory.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFollower : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private UIInventoryItem item;

    public void Awake()
    {
        canvas = transform.root.GetComponent<Canvas>();
        item = GetComponentInChildren<UIInventoryItem>();
        DontDestroyOnLoad(gameObject);
    }

    public void SetData(Sprite sprite, int quantity)
    {
        item.SetData(sprite, quantity);
    }

    void Update()
    {
        Vector2 screenPosition;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // Prefer new Input System
        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            // If primary touch is pressed, use its position
            if (primary.press != null && primary.press.isPressed)
            {
                screenPosition = primary.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                screenPosition = Mouse.current.position.ReadValue();
            }
            else
            {
                // Fallback to legacy Input.mousePosition in case neither device is available
                screenPosition = Input.mousePosition;
            }
        }
        else if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            screenPosition = Input.mousePosition;
        }
#else
        // Legacy input system
        if (Input.touchCount > 0)
        {
            screenPosition = Input.GetTouch(0).position;
        }
        else
        {
            screenPosition = Input.mousePosition;
        }
#endif

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            screenPosition,
            canvas.worldCamera,
            out localPoint
        );
        transform.position = canvas.transform.TransformPoint(localPoint);
    }

    public void Toggle(bool val)
    {
        Debug.Log($"Item toggled {val}");
        gameObject.SetActive(val);
    }
}
