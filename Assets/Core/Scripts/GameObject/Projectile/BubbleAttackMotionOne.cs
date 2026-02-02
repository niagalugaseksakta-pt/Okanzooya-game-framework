using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BubblePatternController : MonoBehaviour
{
    [Header("=== ARRAY PREFAB ===")]
    public GameObject[] projectilePrefabs;

    [Header("=== GENERAL SETTINGS ===")]
    public float maxRadius = 4f;
    public float expandDuration = 0.8f;
    public float driftStrength = 0.4f;
    public float lifetime = 3.5f;
    public float fadeOutDuration = 0.8f;

    [Header("=== ANGLE SETTINGS ===")]
    public float stepAngle = 15f;

    [Header("=== CAMERA LIMIT ===")]
    public float cameraBoundPadding = 1f;

    [Header("=== PATTERN SETTINGS ===")]
    public PatternMode pattern;
    public enum PatternMode
    {
        RadiusBurst,
        Spiral,
        RandomScatter,
        LineBarrage
    }

    [Header("=== LINE BARRAGE ===")]
    public float lineSpacing = 0.8f;
    public int lineCount = 5;
    public float lineLength = 15f;

    [Header("=== BURST LOOP ===")]
    public bool autoRepeat = false;
    public float repeatDelay = 1f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        StartCoroutine(BurstLoop());
    }

    IEnumerator BurstLoop()
    {
        while (true)
        {
            Execute();

            if (!autoRepeat)
                break;

            yield return new WaitForSeconds(repeatDelay);
        }
    }

    // ============================================================
    //  CALLED FROM BOSS ATTACK
    // ============================================================
    public void Execute()
    {
        if (projectilePrefabs == null || projectilePrefabs.Length == 0)
        {
            Debug.LogError("❌ projectilePrefabs kosong!");
            return;
        }

        switch (pattern)
        {
            case PatternMode.RadiusBurst: SpawnRadiusBurst(); break;
            case PatternMode.Spiral: SpawnSpiral(); break;
            case PatternMode.RandomScatter: SpawnRandomScatter(); break;
            case PatternMode.LineBarrage: SpawnLineBarrage(); break;
        }
    }

    GameObject GetRandomPrefab() => projectilePrefabs[Random.Range(0, projectilePrefabs.Length)];

    // ============================================================
    //  PATTERN 1: RADIUS BURST
    // ============================================================
    void SpawnRadiusBurst()
    {
        for (float angle = 0; angle < 360f; angle += stepAngle)
        {
            Vector3 dir = AngleToDir(angle);
            SpawnProjectile(dir, dir * maxRadius);
        }
    }

    // ============================================================
    //  PATTERN 2: SPIRAL
    // ============================================================
    void SpawnSpiral()
    {
        float angle = 0;

        for (int i = 0; i < 24; i++)
        {
            Vector3 dir = AngleToDir(angle);
            angle += stepAngle + i * 2f;

            SpawnProjectile(dir, dir * maxRadius);
        }
    }

    // ============================================================
    //  PATTERN 3: RANDOM SCATTER
    // ============================================================
    void SpawnRandomScatter()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            SpawnProjectile(dir, dir * maxRadius * Random.Range(0.6f, 1.2f));
        }
    }

    // ============================================================
    //  PATTERN 4: LINE BARRAGE (laser style)
    // ============================================================
    void SpawnLineBarrage()
    {
        float half = (lineCount - 1) * lineSpacing * 0.5f;

        for (int i = 0; i < lineCount; i++)
        {
            float offset = -half + (i * lineSpacing);
            Vector3 pos = transform.position + new Vector3(0, offset, 0);

            Vector3 target = pos + Vector3.right * lineLength;

            SpawnProjectile(Vector3.right, target - pos, pos);
        }
    }

    // ============================================================
    //  PROJECTILE LOGIC
    // ============================================================
    void SpawnProjectile(Vector3 dir, Vector3 offset, Vector3? overridePos = null)
    {
        // Camera clipping check
        if (!IsInsideCamera(transform.position + offset))
            return;

        GameObject pf = GetRandomPrefab();
        GameObject go = Instantiate(pf, overridePos ?? transform.position, Quaternion.identity);

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();

        // Rotate continuously
        go.transform.DORotate(new Vector3(0, 0, 360f), 0.6f, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetEase(Ease.Linear);

        // Move outward
        go.transform.DOMove(transform.position + offset, expandDuration)
            .SetEase(Ease.OutQuad);

        // Apply drift
        go.transform.DOMove(transform.position + offset + dir * driftStrength, lifetime)
            .SetEase(Ease.OutSine);

        // Physics kick
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.AddForce(dir * 2f, ForceMode2D.Impulse);
        }

        // Fade out
        if (sr != null)
        {
            sr.DOFade(0f, fadeOutDuration)
                .SetDelay(lifetime - fadeOutDuration)
                .OnComplete(() => Destroy(go));
        }
    }

    // ============================================================
    //  UTILITY
    // ============================================================
    Vector3 AngleToDir(float angle)
    {
        float r = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(r), Mathf.Sin(r), 0);
    }

    bool IsInsideCamera(Vector3 pos)
    {
        if (mainCam == null) return true;

        Vector3 vp = mainCam.WorldToViewportPoint(pos);

        return vp.x > 0 + cameraBoundPadding &&
               vp.x < 1 - cameraBoundPadding &&
               vp.y > 0 + cameraBoundPadding &&
               vp.y < 1 - cameraBoundPadding;
    }
}
