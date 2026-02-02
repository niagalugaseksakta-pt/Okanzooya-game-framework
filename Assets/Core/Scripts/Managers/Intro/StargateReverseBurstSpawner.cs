using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;


public class StargateReverseBurstSpawner_Extreme : MonoBehaviour
{
    // =====================================================
    // ⭐ NEW — GLOBAL SPEED SCALE
    // =====================================================
    [Header("Global Control")]
    [Tooltip("Global speed multiplier affecting all motion, scale, rotation, turbulence & timing.")]
    public float speedScale = 1f;

    public enum StargateMode { Tween, Cinematic }

    [Header("Mode")]
    public StargateMode mode = StargateMode.Tween;

    [Header("Prefab")]
    public GameObject warpPrefab;

    [Header("Burst Settings")]
    public float radius = 150f;
    public float endZ = 0f;
    public float startZ = 0f;

    [Header("Timing")]
    public float spawnInterval = 0.25f;
    public float burstDuration = 5f;
    public float lifeMultiplier = 1.1f;

    [Header("Scale Settings")]
    public float startScale = 0.02f;
    public float endScale = 8f;
    public bool vanishAtEnd = true;

    [Header("Rotation Settings")]
    public bool randomRotation = true;
    public float spinMin = 240f;
    public float spinMax = 3600f;

    [Header("Chaos Controls")]
    public float directionChaos = 0.75f;
    public float turbulenceStrength = 3f;
    public float turbulenceVibrato = 25f;
    public float suctionStrength = 0.35f;

    [Header("Cinematic Controls")]
    public bool useTripleShockwave = true;
    public float shockwaveScale1 = 0.45f;
    public float shockwaveScale2 = 0.75f;
    public float shockwaveScale3 = 1.0f;

    [Header("Vortex")]
    public bool enableVortex = true;
    public float vortexIntensity = 0.95f;
    public float vortexLoops = 2f;

    private bool running = true;
    [SerializeField] private float waitTime = 1f;

    // =====================================================
    // 🧠 RUNTIME TRACKING
    // =====================================================
    private readonly List<GameObject> spawnedObjects = new();


    private void Start()
    {
        DOTween.Init();
        StartCoroutine(SpawnLoop());
    }

    private System.Collections.IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(waitTime);
        while (running)
        {
            Spawn();
            yield return new WaitForSeconds(spawnInterval * (1f / speedScale));
        }
    }

    private void Spawn()
    {
        if (!warpPrefab) return;

        GameObject obj = Instantiate(warpPrefab);
        obj.transform.position = new Vector3(transform.position.x, transform.position.y, endZ);
        obj.transform.localScale = Vector3.one * startScale;

        spawnedObjects.Add(obj);

        SpriteRenderer[] children = obj.GetComponentsInChildren<SpriteRenderer>();

        if (children.Length > 0)
        {
            int pick = Random.Range(0, children.Length);
            for (int i = 0; i < children.Length; i++)
            {
                children[i].enabled = (i == pick);
                children[i].sortingOrder = 10;
            }
        }

        switch (mode)
        {
            case StargateMode.Tween:
                PlayTweenMode(obj);
                break;

            case StargateMode.Cinematic:
                PlayCinematicMode(obj);
                break;
        }

        float finalLife = (burstDuration * lifeMultiplier) * (1f / speedScale);
        StartCoroutine(AutoCleanup(obj, finalLife));

    }

    private System.Collections.IEnumerator AutoCleanup(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!obj) yield break;

        DOTween.Kill(obj.transform);
        spawnedObjects.Remove(obj);
        Destroy(obj);
    }

    public void ClearSpawned(bool instant = false)
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjects[i];
            if (!obj) continue;

            DOTween.Kill(obj.transform);

            if (instant)
            {
                Destroy(obj);
            }
            else
            {
                obj.transform
                    .DOScale(0f, 0.35f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(obj));
            }
        }

        spawnedObjects.Clear();
    }

    public void StopSpawning()
    {
        running = false;
    }

    public void ResumeSpawning()
    {
        if (running) return;
        running = true;
        StartCoroutine(SpawnLoop());
    }

    public void ClearAndStop(bool instant = true)
    {
        StopSpawning();
        ClearSpawned(instant);
    }


    // =====================================================
    // MODE 1 — CLEAN TWEEN VERSION
    // =====================================================
    private void PlayTweenMode(GameObject obj)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);

        Vector3 target = transform.position + new Vector3(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius,
            startZ
        );

        float dur = burstDuration * (1f / speedScale);

        obj.transform.DOMove(target, dur)
            .SetEase(Ease.OutQuad);

        obj.transform.DOScale(endScale, dur)
            .SetEase(Ease.OutBack);

        if (randomRotation)
        {
            obj.transform.DORotate(
                new Vector3(0, 0, Random.Range(spinMin, spinMax)),
                dur,
                RotateMode.FastBeyond360
            ).SetEase(Ease.Linear);
        }
    }


    // =====================================================
    // MODE 2 — CINEMATIC CHAOS
    // =====================================================
    private void PlayCinematicMode(GameObject obj)
    {
        float baseAngle = Random.Range(0f, Mathf.PI * 2f);
        float chaos = Random.Range(-directionChaos, directionChaos);
        float angle = baseAngle + chaos;

        float dur = burstDuration * (1f / speedScale);
        float suctionDur = suctionStrength * (1f / speedScale);

        Vector3 target = new Vector3(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius,
            startZ
        );

        // ==========================================================
        // SUCTION (scaled)
        // ==========================================================
        obj.transform.DOMove(transform.position, suctionDur)
            .SetEase(Ease.InBack);

        obj.transform.DOPunchScale(Vector3.one * 0.5f, suctionDur, 14, 2f);

        // ==========================================================
        // BURST
        // ==========================================================
        obj.transform.DOMove(target, dur)
            .SetEase(Ease.OutExpo)
            .SetDelay(0.05f * (1f / speedScale));

        // ==========================================================
        // TRIPLE SHOCKWAVE SCALE
        // ==========================================================
        Sequence scaleSeq = DOTween.Sequence();

        if (useTripleShockwave)
        {
            scaleSeq.Append(obj.transform.DOScale(endScale * shockwaveScale1, dur * 0.25f).SetEase(Ease.OutElastic));
            scaleSeq.Append(obj.transform.DOScale(endScale * shockwaveScale2, dur * 0.35f).SetEase(Ease.InOutBack));
            scaleSeq.Append(obj.transform.DOScale(endScale * shockwaveScale3, dur * 0.40f).SetEase(Ease.OutCirc));
        }
        else
        {
            scaleSeq.Append(obj.transform.DOScale(endScale, dur).SetEase(Ease.OutCirc));
        }

        if (vanishAtEnd)
        {
            scaleSeq.Append(obj.transform.DOScale(0.001f, dur * 0.25f).SetEase(Ease.InBack));
        }

        // ==========================================================
        // ROTATION
        // ==========================================================
        if (randomRotation)
        {
            obj.transform.DORotate(
                new Vector3(0, 0, Random.Range(spinMin, spinMax)),
                dur,
                RotateMode.FastBeyond360
            ).SetEase(Ease.Linear);
        }

        // ==========================================================
        // TURBULENCE SHAKE (scaled)
        // ==========================================================
        obj.transform.DOShakePosition(
            dur * 0.6f,
            turbulenceStrength,
            Mathf.RoundToInt(turbulenceVibrato),
            110
        );

        // ==========================================================
        // VORTEX (scaled)
        // ==========================================================
        if (enableVortex)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 swirl = Quaternion.Euler(0, 0, 90f) * dir * vortexIntensity;

            obj.transform.DOLocalMove(swirl, dur * 0.5f)
                .SetEase(Ease.OutSine)
                .SetLoops(Mathf.RoundToInt(vortexLoops), LoopType.Yoyo);
        }
    }
}
