using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform handle;
    [SerializeField] private RectTransform background;

    [Header("Settings")]
    [SerializeField] private float handleLimit = 100f;

    private Vector2 inputVector = Vector2.zero;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;
    public Vector2 Direction => new Vector2(Horizontal, Vertical);

    private Vector2 startPos;

    void Start()
    {
        if (background == null)
            background = GetComponent<RectTransform>();

        startPos = handle.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        pos = Vector2.ClampMagnitude(pos, handleLimit);
        handle.anchoredPosition = pos;

        inputVector = pos / handleLimit;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = startPos;
        inputVector = Vector2.zero;
    }
}
