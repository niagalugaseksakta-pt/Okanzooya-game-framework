using System;
using System.Collections.Generic;
using UnityEngine;
using static Game.Config.Config;

public class Emoticon : MonoBehaviour
{
    private Animator anim;
    private Dictionary<EmoteState, string> map;
    private HashSet<string> availableClips;

    private void Awake()
    {
        gameObject.SetActive(false);
        anim = GetComponent<Animator>();
        BuildMap();
        CacheClips();

        if (anim == null)
            Debug.LogWarning("Animator tidak ditemukan!", this);
    }

    /// <summary>
    /// Bangun map enum → nama clip.
    /// </summary>
    private void BuildMap()
    {
        map = new Dictionary<EmoteState, string>();

        foreach (EmoteState s in Enum.GetValues(typeof(EmoteState)))
        {
            string clip = s.ToString().Replace("_", "-");
            map[s] = clip;
        }
    }

    /// <summary>
    /// Cache semua animation clip dari Animator.
    /// </summary>
    private void CacheClips()
    {
        availableClips = new HashSet<string>();

        if (anim.runtimeAnimatorController == null)
            return;

        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            availableClips.Add(clip.name);
        }
    }

    /// <summary>
    /// Memainkan emote berdasarkan Animator state.
    /// </summary>
    public void PlayEmote(EmoteState state)
    {
        if (!map.TryGetValue(state, out string clipName))
        {
            Debug.LogWarning($"Emote '{state}' tidak ditemukan!", this);
            return;
        }

        if (!availableClips.Contains(clipName))
        {
            Debug.LogWarning($"Animator tidak memiliki clip '{clipName}'!", this);
            return;
        }

        gameObject.SetActive(true);
        anim.Play(clipName, 0, 0f);
    }

    /// <summary>
    /// Mengecek apakah clip tertentu sedang dimainkan oleh Animator.
    /// </summary>
    public bool IsPlaying(EmoteState state)
    {
        if (!map.TryGetValue(state, out string clipName))
            return false;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        return info.IsName(clipName);
    }
}
