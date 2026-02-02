using Assets.CoreFramework.Scripts.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Game.Config.Config;

public class LoadingController : BaseImportantReuseableComponent
{
    [Header("Puzzle Pieces")]
    [SerializeField] private List<Image> puzzlePieces;       // assign in Inspector
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.white;

    [Header("Text")]
    [SerializeField] private TMP_Text loadingText;

    private GameEventBus _bus;

    private float _raw = 0f;
    private float _stable = 0f;
    private float _smooth = 0f;
    private bool _isDoneLoad = false;
    private string _msg;

    // motion constants
    private const float STABLE_SPEED = 1.6f;
    private const float SMOOTH_SPEED = 6f;
    private const float SNAP = 0.01f;

    private bool[] pieceActivated;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _bus = GameEventBus.Instance;
        _bus.Subscribe<GameLoading>(OnEvent);

        pieceActivated = new bool[puzzlePieces.Count];

        // initialize puzzle
        foreach (var img in puzzlePieces)
            img.color = inactiveColor;
    }

    private void OnEvent(GameLoading evt)
    {
        _raw = evt._targetProgress;
        _isDoneLoad = evt._isDoneLoad;
        _msg = evt._message;

        loadingText.text = _msg;
    }

    private void Update()
    {
        float target = _isDoneLoad ? 1f : _raw;

        // 1) stable buffer
        if (Mathf.Abs(_stable - target) > SNAP)
            _stable = Mathf.MoveTowards(_stable, target, STABLE_SPEED * Time.deltaTime);
        else
            _stable = target;

        // 2) smooth UI progress
        if (Mathf.Abs(_smooth - _stable) > SNAP)
            _smooth = Mathf.Lerp(_smooth, _stable, Time.deltaTime * SMOOTH_SPEED);
        else
            _smooth = _stable;

        // 3) update puzzle pieces
        UpdatePuzzleVisual(_smooth);
    }

    private void UpdatePuzzleVisual(float progress)
    {
        int total = puzzlePieces.Count;

        for (int i = 0; i < total; i++)
        {
            float threshold = (i + 1) / (float)total;

            // piece hasn't activated yet, but progress has reached threshold
            if (!pieceActivated[i] && progress >= threshold)
            {
                pieceActivated[i] = true;
                AnimatePiece(puzzlePieces[i]);
            }
        }
    }

    // Small scale bump + color reveal
    private void AnimatePiece(Image img)
    {
        img.color = activeColor;
        StartCoroutine(PiecePop(img));
    }

    private System.Collections.IEnumerator PiecePop(Image img)
    {
        Vector3 start = img.rectTransform.localScale;
        Vector3 peak = start * 1.35f;

        float t = 0f;

        // scale up
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            img.rectTransform.localScale = Vector3.Lerp(start, peak, t);
            yield return null;
        }

        t = 0f;
        // scale back down
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            img.rectTransform.localScale = Vector3.Lerp(peak, start, t);
            yield return null;
        }

        img.rectTransform.localScale = start;
    }
}
