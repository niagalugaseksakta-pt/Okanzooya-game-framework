using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static Game.Config.Config;

public class StepSequenceProducer : MonoBehaviour, IPointerDownHandler
{
    [System.Serializable]
    public class Step
    {
        [Header("Step Object")]
        public GameObject targetObject;

        [Header("Texts (Sequence in One Step)")]
        [TextArea(2, 5)]
        public string[] texts;

        [Header("Wait After Text")]
        public float waitTime = 2f;

        [Header("DOTween Effect")]
        public TweenEffect tweenEffect = TweenEffect.FadeAndScale;

        public float duration = 0.5f;
        public Ease ease = Ease.OutBack;

        [Header("Optional Values")]
        public Vector3 startScale = new Vector3(0.9f, 0.9f, 0.9f);
        public Vector3 endScale = Vector3.one;
        public float slideDistance = 1.5f;
        public float shakeStrength = 0.2f;

        // Runtime: hold references to active tweens so we can manipulate them on input
        [System.NonSerialized]
        public Tween[] activeTweens;
    }

    // =========================================================
    [Header("Steps")]
    public Step[] steps;

    [Header("UI")]
    public CanvasGroup canvas;
    public TextMeshProUGUI textUI;

    [Header("Typing")]
    public float typingSpeed = 0.03f;
    public bool allowSkip = true;
    public bool disableCanvasAfterFinish = true;

    // =========================================================
    private int index = -1;
    private bool isTyping;
    private bool isWaiting;
    private bool finished;

    private Coroutine sequenceRoutine;
    private Coroutine typingRoutine;
    private string fullText;

    private NextStageController nextStageController;
    // =========================================================
    private void Awake()
    {
        Prepare();
        nextStageController = GetComponent<NextStageController>();
        sequenceRoutine = StartCoroutine(RunSequence());
    }

    private void Prepare()
    {
        finished = false;
        isTyping = false;
        isWaiting = false;

        canvas.alpha = 1f;
        canvas.blocksRaycasts = true;
        canvas.interactable = true;

        textUI.text = "";

        foreach (var step in steps)
        {
            if (step.targetObject != null)
                step.targetObject.SetActive(false);

            // Ensure no leftover tween references
            if (step != null)
                step.activeTweens = null;
        }
    }

    // =========================================================
    private IEnumerator RunSequence()
    {
        yield return new WaitForSeconds(cameraWaitToplayer);

        for (int i = 0; i < steps.Length; i++)
            yield return RunStep(i);

        Finish();
    }

    private IEnumerator RunStep(int stepIndex)
    {
        // Disable previous object
        if (index >= 0 && index < steps.Length)
        {
            if (steps[index].targetObject != null)
            {
                steps[index].targetObject.SetActive(false);

                // Kill and clear any tweens for previous step
                if (steps[index].activeTweens != null)
                {
                    foreach (var t0 in steps[index].activeTweens)
                        if (t0 != null && t0.IsActive())
                            t0.Kill(false);
                    steps[index].activeTweens = null;
                }
            }
        }

        index = stepIndex;
        Step step = steps[stepIndex];

        // === ENABLE OBJECT FIRST ===
        if (step.targetObject != null)
        {
            step.targetObject.SetActive(true);
            PlayTween(step);
        }

        // === TYPE TEXT AFTER OBJECT ===
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        yield return RunTextSequence(step);

        // === WAIT ===
        isWaiting = true;
        float t = 0f;

        while (t < step.waitTime)
        {
            if (!isWaiting) break;
            t += Time.deltaTime;
            yield return null;
        }

        isWaiting = false;
    }

    private IEnumerator RunTextSequence(Step step)
    {
        if (step.texts == null || step.texts.Length == 0)
            yield break;

        for (int i = 0; i < step.texts.Length; i++)
        {
            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            typingRoutine = StartCoroutine(TypeText(step.texts[i]));
            yield return new WaitUntil(() => !isTyping);

            // wait after each text (tap to skip still works)
            isWaiting = true;
            float t = 0f;

            while (t < step.waitTime)
            {
                if (!isWaiting) break;
                t += Time.deltaTime;
                yield return null;
            }

            isWaiting = false;
        }
    }

    private void PlayTween(Step step)
    {
        if (step.targetObject == null || step.tweenEffect == TweenEffect.None)
            return;

        Transform tr = step.targetObject.transform;
        SpriteRenderer sr = step.targetObject.GetComponent<SpriteRenderer>();

        tr.DOKill();
        if (sr != null) sr.DOKill();

        Vector3 originalPos = tr.localPosition;

        // Clear previous stored tweens
        step.activeTweens = null;

        Tween t1 = null;
        Tween t2 = null;

        switch (step.tweenEffect)
        {
            case TweenEffect.FadeIn:
                if (sr == null) break;

                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, 0f);
                t1 = sr.DOFade(1f, step.duration).SetEase(step.ease);
                break;

            case TweenEffect.ScalePop:
                tr.localScale = step.startScale;
                t1 = tr.DOScale(step.endScale, step.duration).SetEase(step.ease);
                break;

            case TweenEffect.FadeAndScale:
                if (sr != null)
                {
                    Color fc = sr.color;
                    sr.color = new Color(fc.r, fc.g, fc.b, 0f);
                    t1 = sr.DOFade(1f, step.duration);
                }
                tr.localScale = step.startScale;
                t2 = tr.DOScale(step.endScale, step.duration).SetEase(step.ease);
                break;

            case TweenEffect.SlideFromLeft:
                tr.localPosition = originalPos + Vector3.left * step.slideDistance;
                t1 = tr.DOLocalMove(originalPos, step.duration).SetEase(step.ease);
                break;

            case TweenEffect.SlideFromRight:
                tr.localPosition = originalPos + Vector3.right * step.slideDistance;
                t1 = tr.DOLocalMove(originalPos, step.duration).SetEase(step.ease);
                break;

            case TweenEffect.Shake:
                tr.localPosition = originalPos;
                t1 = tr.DOShakePosition(step.duration, step.shakeStrength);
                break;
            default: break;
        }

        // Store any created tweens for runtime control
        if (t1 != null && t2 != null)
            step.activeTweens = new Tween[] { t1, t2 };
        else if (t1 != null)
            step.activeTweens = new Tween[] { t1 };
        else if (t2 != null)
            step.activeTweens = new Tween[] { t2 };
        else
            step.activeTweens = null;
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;
        textUI.text = "";

        foreach (char c in fullText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingRoutine = null;
    }

    // =========================================================
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!allowSkip || finished)
            return;

        // 1) If current step has active tweens, finish them to the step's duration and return.
        if (index >= 0 && index < steps.Length)
        {
            var step = steps[index];
            if (step != null && step.activeTweens != null)
            {
                bool advanced = FinishCurrentTweensByDuration(step.duration);
                if (advanced)
                    return;
            }
        }

        // 2) If typing, complete typing immediately.
        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        // 3) If waiting, cancel waiting.
        if (isWaiting)
        {
            isWaiting = false;
            return;
        }

        // 4) Otherwise skip to next step in the sequence.
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = StartCoroutine(RunFrom(index + 1));
        }
    }

    // Advance current step tweens to a specific target time (or to their full duration if 0 or negative passed)
    private bool FinishCurrentTweensByDuration(float targetDuration)
    {
        if (index < 0 || index >= steps.Length)
            return false;

        Step step = steps[index];
        if (step == null || step.activeTweens == null || step.activeTweens.Length == 0)
            return false;

        bool anyProcessed = false;

        foreach (var tween in step.activeTweens)
        {
            if (tween == null || !tween.IsActive())
                continue;

            float durationToUse = targetDuration;
            if (durationToUse <= 0f)
                durationToUse = tween.Duration();

            // Move tween to the given time and update the target immediately
            tween.Goto(durationToUse, true);
            anyProcessed = true;
        }

        // Optionally clear references if all tweens are complete
        // (check first tween state; if all are complete, null them)
        bool allComplete = true;
        foreach (var tween in step.activeTweens)
        {
            if (tween != null && tween.IsActive() && !tween.IsComplete())
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
            step.activeTweens = null;

        return anyProcessed;
    }

    private void CompleteTyping()
    {
        if (!isTyping) return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        textUI.text = fullText;
        isTyping = false;
        typingRoutine = null;
    }

    private IEnumerator RunFrom(int start)
    {
        for (int i = start; i < steps.Length; i++)
            yield return RunStep(i);

        Finish();
    }

    // =========================================================
    private void Finish()
    {
        finished = true;
        textUI.text = "";

        if (index >= 0 && index < steps.Length)
        {
            if (steps[index].targetObject != null)
                steps[index].targetObject.SetActive(false);

            // Kill any remaining tweens for the current step
            if (steps[index].activeTweens != null)
            {
                foreach (var t in steps[index].activeTweens)
                    if (t != null && t.IsActive())
                        t.Kill(false);
                steps[index].activeTweens = null;
            }
        }

        if (disableCanvasAfterFinish)
        {
            canvas.alpha = 0f;
            canvas.blocksRaycasts = false;
            canvas.interactable = false;
        }

        nextStageController.GoToSelectedStage();
    }
}
