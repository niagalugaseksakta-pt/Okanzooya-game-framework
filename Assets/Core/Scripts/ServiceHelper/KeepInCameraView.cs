using DG.Tweening; // Optional if you want smooth movement
using UnityEngine;

[DisallowMultipleComponent]
public class KeepInCameraView : MonoBehaviour
{
    [Header("Settings")]
    public Camera targetCamera;
    public bool useTween = true;
    public float tweenDuration = 0.15f;
    public bool includeOffscreenCheck = true;

    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (includeOffscreenCheck == false)
            return;

        if (canvas != null &&
            (canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
             canvas.renderMode == RenderMode.ScreenSpaceCamera))
        {
            KeepUIInsideScreen();
        }
        else
        {
            KeepWorldObjectInsideCamera();
        }
    }

    private void KeepUIInsideScreen()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector2 min = corners[0];
        Vector2 max = corners[2];

        Vector2 offset = Vector2.zero;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (min.x < 0)
            offset.x = -min.x;
        else if (max.x > screenWidth)
            offset.x = screenWidth - max.x;

        if (min.y < 0)
            offset.y = -min.y;
        else if (max.y > screenHeight)
            offset.y = screenHeight - max.y;

        if (offset != Vector2.zero)
        {
            Vector2 targetPos = rectTransform.anchoredPosition + offset;

            if (useTween)
                rectTransform.DOAnchorPos(targetPos, tweenDuration).SetEase(Ease.OutQuad);
            else
                rectTransform.anchoredPosition = targetPos;
        }
    }

    private void KeepWorldObjectInsideCamera()
    {
        if (targetCamera == null) return;

        Vector3 screenPos = targetCamera.WorldToScreenPoint(transform.position);

        // Object behind camera? Don’t move it.
        if (screenPos.z < 0) return;

        float clampedX = Mathf.Clamp(screenPos.x, 0, Screen.width);
        float clampedY = Mathf.Clamp(screenPos.y, 0, Screen.height);

        Vector3 clampedWorldPos = targetCamera.ScreenToWorldPoint(
            new Vector3(clampedX, clampedY, screenPos.z)
        );

        if (useTween)
            transform.DOMove(clampedWorldPos, tweenDuration).SetEase(Ease.OutQuad);
        else
            transform.position = clampedWorldPos;
    }
}
