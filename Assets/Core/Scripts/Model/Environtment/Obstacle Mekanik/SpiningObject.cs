using DG.Tweening;
using UnityEngine;

public class SpiningObject : EntityObstacleBase
{
    [Header("Spin Settings")]
    public float rotationSpeed = 180f; // derajat per detik
    public Vector3 rotationAxis = Vector3.up;
    public bool playOnStart = true;
    public Ease easeType = Ease.Linear;

    private Tween spinTween;

    protected override void Start()
    {
        if (playOnStart)
            PlaySpin();
    }


    public void PlaySpin()
    {
        // Hitung durasi berdasarkan kecepatan (180°/detik → 2 detik untuk 360°)
        float duration = 360f / rotationSpeed;

        spinTween = transform
            .DORotate(rotationAxis * 360f, duration, RotateMode.FastBeyond360)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Restart);
    }

    public void StopSpin()
    {
        spinTween?.Kill();
    }
}
