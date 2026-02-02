using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IEndDragHandler,
    IDragHandler, IDropHandler
{
    [Header("Settings")]
    [SerializeField] private float longPressThreshold = 0.4f;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image itemImage;

    private bool isDragging;
    private bool isLongPressTriggered;
    private float pointerDownTime;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        itemImage = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownTime = Time.time;
        isLongPressTriggered = false;
        StartCoroutine(CheckLongPress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isLongPressTriggered && !isDragging)
        {
            Debug.Log($"Short Click on {name}");
            // Example: open item info
        }
    }

    private IEnumerator CheckLongPress()
    {
        yield return new WaitForSeconds(longPressThreshold);
        if (!isDragging)
        {
            isLongPressTriggered = true;
            Debug.Log($"Long Press on {name}");
            // Example: show tooltip or item detail
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = eventData.pointerDrag?.GetComponent<ItemDragHandler>();
        if (draggedItem != null && draggedItem != this)
        {
            Debug.Log($"Swap {draggedItem.name} with {name}");

            // Simple visual swap:
            Sprite tempSprite = itemImage.sprite;
            itemImage.sprite = draggedItem.itemImage.sprite;
            draggedItem.itemImage.sprite = tempSprite;
        }
    }
}
