using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemsLoader : MonoBehaviour
{
    [Header("Addressable Labels or Keys")]
    [SerializeField] private string labelOrFolder = "Carb";

    [Header("Movement Settings")]
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private float scale = 0.9f;
    [SerializeField] private Vector3 startDirection = Vector3.right;

    [Header("Visual Feedback")]
    [SerializeField] private float unmatchShakeIntensity = 0.3f;
    [SerializeField] private float unmatchShakeTime = 0.25f;
    [SerializeField] private Color unmatchFlashColor = Color.red;
    [SerializeField] private CanvasGroup screenFlash; // optional overlay flash (assign UI Image alpha=0)
    [SerializeField] private float reorderTweenTime = 0.35f;

    private readonly List<GameObject> marbles = new();
    private readonly Queue<GameObject> pool = new();
    private Vector3 startPosition;

    public List<GameObject> GetMarbles() => marbles;
    private AsyncOperationHandle<IList<Texture2D>> handle;
    public AsyncOperationHandle<IList<Texture2D>> HandleLoadChecker() => handle;


    void Start()
    {
        startPosition = transform.position;

    }


    public async Task<List<GameObject>> LoadAndCreateMarblesAsync()
    {
        handle = Addressables.LoadAssetsAsync<Texture2D>(labelOrFolder, null);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"❌ Addressable load failed for label: {labelOrFolder}");
            return null;
        }

        var textures = handle.Result;

        // 🧩 Sort by name for predictable order
        var sorted = new List<Texture2D>(textures);
        sorted.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));

        // 🪄 Use sorted list instead of unsorted one
        for (int i = 0; i < sorted.Count; i++)
        {
            var tex = sorted[i];
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            var marble = new GameObject($"Marble_{i}_{tex.name}");
            var renderer = marble.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "MatchingItems";
            renderer.sortingOrder = 10000 + i;

            marble.transform.localScale = Vector3.one * scale;
            marble.transform.position = startPosition + startDirection * (i * spacing);

            marbles.Add(marble);
        }

        return marbles;
    }


    private void ReleaseAnimation(GameObject marble)
    {
        if (marble == null) return;

        var renderer = marble.GetComponent<SpriteRenderer>();
        if (renderer) renderer.sortingOrder = 20000; // force front

        // shrink & fade
        var seq = DOTween.Sequence();
        seq.Append(marble.transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack));
        if (renderer)
            seq.Join(renderer.DOFade(0, 0.35f));
        seq.OnComplete(() =>
        {
            marble.SetActive(false);
            pool.Enqueue(marble);
        });
    }

    private void ReorderMarbles()
    {
        for (int i = 0; i < marbles.Count; i++)
        {
            var m = marbles[i];
            if (!m) continue;

            Vector3 pos = startPosition + startDirection * (i * spacing);
            m.transform.DOMove(pos, reorderTweenTime).SetEase(Ease.OutQuad);

            var r = m.GetComponent<SpriteRenderer>();
            if (r) r.sortingOrder = 10000 + i; // ✅ stable increasing order
        }
    }

    // ---------------- TEST KEYS ----------------
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.M))
        //    RemoveFirst();

        //if (Input.GetKeyDown(KeyCode.N))
        //    TestUnmatchEffect();
    }

    // ✅ MATCH (remove)
    public void RemoveFirst()
    {
        if (marbles.Count == 0)
        {
            Debug.Log("No marbles to remove.");
            return;
        }

        GameObject first = marbles[0];
        marbles.RemoveAt(0);
        ReleaseAnimation(first);
        ReorderMarbles();
    }

    // ❌ UNMATCH (shake, flash, highlight)
    private void TestUnmatchEffect()
    {
        if (marbles.Count == 0) return;

        //int index = Random.Range(0, marbles.Count);
        var target = marbles[0];
        var renderer = target.GetComponent<SpriteRenderer>();

        // 🔥 Shake & pulse color
        target.transform.DOShakePosition(unmatchShakeTime, unmatchShakeIntensity);
        if (renderer != null)
        {
            var origColor = renderer.color;
            var pulseSeq = DOTween.Sequence();
            pulseSeq.Append(renderer.DOColor(unmatchFlashColor, 0.1f));
            pulseSeq.Append(renderer.DOColor(origColor, 0.25f));
            pulseSeq.Join(renderer.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1));
        }

        // 💥 Screen flash
        if (screenFlash != null)
        {
            screenFlash.DOFade(0.7f, 0.05f)
                .OnComplete(() => screenFlash.DOFade(0, 0.4f));
        }

        // 🎥 Camera shake + reset
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var camSeq = DOTween.Sequence();
            camSeq.Append(mainCam.transform.DOShakePosition(0.25f, 0.4f, 15, 90, false, false, ShakeRandomnessMode.Full));
            camSeq.Append(mainCam.transform.DOLocalMove(new Vector3(0, -2.5f, -10f), 0.1f)); // ✅ snap back cleanly
        }
    }

    private void OnDestroy()
    {
        if (handle.IsValid()) Addressables.Release(handle);
    }
}
