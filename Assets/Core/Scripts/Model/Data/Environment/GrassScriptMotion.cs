using Game.Model.Player;
using System.Collections;
using UnityEngine;

public class GrassScriptMotion : MonoBehaviour
{
    private PlayerEntity playerEntity;
    private Animator animator;
    private Coroutine shakeCoroutine;
    private const string ShakeStateName = "Shake";

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }

        // Keep existing behaviour (original code used DontDestroyOnLoad)
        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsColliderPlayer(collision))
            return;

        StartShaking();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsColliderPlayer(collision))
            return;

        StopShaking();
    }

    private bool IsColliderPlayer(Collider2D collision)
    {
        if (collision == null)
            return false;
        if (playerEntity != null)
            return collision.attachedRigidbody?.gameObject == playerEntity.gameObject || collision.gameObject == playerEntity.gameObject;
        return collision.GetComponent<PlayerEntity>() != null;
    }

    private void StartShaking()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (shakeCoroutine != null)
            return;

        var clip = FindAnimationClipByName(animator, ShakeStateName);
        if (clip == null)
        {
            // Fallback: just play the state and rely on the AnimatorController loop settings
            animator.enabled = true;
            animator.Play(ShakeStateName, 0, 0f);
            return;
        }

        int totalFrames = Mathf.Max(1, Mathf.RoundToInt(clip.length * clip.frameRate));
        int framesToUse = Mathf.Min(2, totalFrames);
        float frameDuration = 1f / Mathf.Max(1f, clip.frameRate);

        shakeCoroutine = StartCoroutine(PlayTwoFrameLoop(clip, totalFrames, framesToUse, frameDuration));
    }

    private void StopShaking()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        // Reset to the first frame of the Shake animation and freeze (so grass is not mid-frame)
        var clip = FindAnimationClipByName(animator, ShakeStateName);
        if (clip != null)
        {
            animator.enabled = true;
            animator.Play(ShakeStateName, 0, 0f);
            // Apply immediately and then freeze animator so it stays on the first frame
            animator.Update(0f);
            animator.enabled = false;
        }
        else
        {
            // If no clip, try to stop playback cleanly
            animator.enabled = false;
        }
    }

    private AnimationClip FindAnimationClipByName(Animator anim, string name)
    {
        if (anim == null || anim.runtimeAnimatorController == null)
            return null;
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == name)
                return clip;
        }
        return null;
    }

    private IEnumerator PlayTwoFrameLoop(AnimationClip clip, int totalFrames, int framesToUse, float frameDuration)
    {
        // Ensure animator is enabled while animating
        animator.enabled = true;

        int current = 0;
        // Use the center of the frame to avoid sampling on exact border: (i + 0.5) / totalFrames
        while (true)
        {
            float normalizedTime = (current + 0.5f) / (float)totalFrames;
            animator.Play(ShakeStateName, 0, normalizedTime);
            // Apply immediately so the sprite/frame updates this frame
            animator.Update(0f);

            yield return new WaitForSeconds(frameDuration);

            current = (current + 1) % framesToUse;
        }
    }

    private void OnDestroy()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
    }
}
