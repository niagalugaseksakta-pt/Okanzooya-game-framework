using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class UIInventoryItem : MonoBehaviour, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IEndDragHandler,
    IDragHandler, IDropHandler
    {
        [SerializeField]
        private Image itemImage;
        [SerializeField]
        private TMP_Text quantityTxt;

        [SerializeField]
        private Image borderImage;

        public event Action<UIInventoryItem> OnItemClicked,
            OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag,
            OnPointerActionShowBtnClick;

        private bool empty = true;

        // Longpress parameters
        [Header("Settings")]
        [SerializeField] private float longPressThreshold = 1f;

        private Canvas canvas;
        private RectTransform rectTransform;
        //[SerializeField] private CanvasGroup canvasGroup;

        private bool isDragging;
        private bool isLongPressTriggered;
        private bool isPointerDown;
        private float pointerDownTime;

        public void Awake()
        {
            canvas = transform.root.GetComponent<Canvas>();
            ResetData();
            Deselect();
            DontDestroyOnLoad(gameObject);
        }

        public Image getImage()
        {
            return itemImage;
        }

        public TMP_Text GetQuantityByTxt()
        {
            return quantityTxt;
        }

        public void ResetData()
        {
            itemImage.gameObject.SetActive(false);
            empty = true;
        }
        public void Deselect()
        {
            borderImage.enabled = false;
        }
        public void SetData(Sprite sprite, int quantity)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            quantityTxt.text = quantity + "";
            empty = false;
        }

        public void Select()
        {
            borderImage.enabled = true;
        }

        // handler long pres
        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
            isLongPressTriggered = false;
            OnItemClicked?.Invoke(this);
            StartCoroutine(CheckLongPress());
        }

        private IEnumerator CheckLongPress()
        {
            float elapsed = 0f;

            while (elapsed < longPressThreshold)
            {
                // if user released early or started dragging → cancel
                if (!isPointerDown || isDragging)
                    yield break;

                elapsed += Time.unscaledDeltaTime; // use unscaled to ignore pause
                yield return null;
            }

            // Still holding, not dragging → trigger
            if (!isDragging && isPointerDown)
            {
                isLongPressTriggered = true;
                Debug.Log($"Long Press on {name}");
                OnPointerActionShowBtnClick?.Invoke(this);
            }
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
            StopCoroutine(CheckLongPress());

            if (!isLongPressTriggered)
            {
                // short click action
                OnItemClicked?.Invoke(this);
            }
        }

        public void OnPointerClick(PointerEventData pointerData)
        {
            //Handle Selection outside long press
            //OnItemClicked?.Invoke(this);

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            //canvasGroup.alpha = 0.6f;
            //canvasGroup.blocksRaycasts = false;
            OnItemEndDrag?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            if (empty)
                return;
            OnItemBeginDrag?.Invoke(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"On Drop {this.name}");
            OnItemDroppedOn?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // this.rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
}