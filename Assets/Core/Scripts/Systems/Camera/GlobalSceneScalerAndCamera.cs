using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GlobalSceneCamera : MonoBehaviour
{
    [Header("🎯 Camera")]
    public Camera mainCamera;

    // ============================================================
    //  🏔 PARALLAX
    // ============================================================

    [Header("🌄 Parallax Layers")]
    public List<Transform> farLayers;
    public List<Transform> midLayers;
    public List<Transform> nearLayers;

    [Range(0f, 2f)] public float farFactor = 0.3f;
    [Range(0f, 2f)] public float midFactor = 0.6f;
    [Range(0f, 2f)] public float nearFactor = 0.85f;

    public float parallaxSmooth = 3f;
    private Vector3 lastCamPos;

    // ============================================================
    //  🌫 FOG SYSTEM
    // ============================================================

    [Header("🌫 Fog Settings")]
    public bool enableFog2D = true;
    public Gradient fogGradient;
    [Range(0f, 1f)] public float fogIntensity = 0.2f;

    void Awake()
    {
        if (!mainCamera) mainCamera = Camera.main;
        lastCamPos = mainCamera.transform.position;
    }

    void LateUpdate()
    {
        if (!mainCamera) return;

        Parallax();
        Fog2D();

        lastCamPos = mainCamera.transform.position;
    }

    // ============================================================
    //  🏔 PARALLAX
    // ============================================================

    void Parallax()
    {
        Vector3 camPos = mainCamera.transform.position;
        Vector3 camDelta = camPos - lastCamPos;

        ApplyParallax(farLayers, camDelta, farFactor);
        ApplyParallax(midLayers, camDelta, midFactor);
        ApplyParallax(nearLayers, camDelta, nearFactor);
    }

    void ApplyParallax(List<Transform> layers, Vector3 delta, float factor)
    {
        if (layers == null) return;

        foreach (var t in layers)
        {
            if (!t) continue;

            Vector3 target = t.position + new Vector3(delta.x * factor, delta.y * factor, 0);
            t.position = Vector3.Lerp(t.position, target, Time.deltaTime * parallaxSmooth);
        }
    }

    // ============================================================
    //  🌫 FOG SYSTEM
    // ============================================================

    void Fog2D()
    {
        if (!enableFog2D || fogGradient == null) return;

        Color farC = fogGradient.Evaluate(0.2f) * fogIntensity;
        Color midC = fogGradient.Evaluate(0.5f) * fogIntensity;
        Color nearC = fogGradient.Evaluate(0.8f) * fogIntensity;

        ApplyFogTint(farLayers, farC);
        ApplyFogTint(midLayers, midC);
        ApplyFogTint(nearLayers, nearC);
    }

    void ApplyFogTint(List<Transform> layers, Color tint)
    {
        if (layers == null) return;

        foreach (var t in layers)
        {
            if (!t) continue;
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.Lerp(sr.color, tint, 0.05f);
        }
    }
}
