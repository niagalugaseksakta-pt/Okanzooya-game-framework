using System.Collections;
using UnityEngine;

public static class AudioFader
{
    public static IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0.01f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Restore for next use
    }

    public static IEnumerator FadeIn(AudioSource audioSource, AudioClip clip, float fadeTime)
    {
        float targetVolume = audioSource.volume; // keep whatever was set in the editor
        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.Play();

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += targetVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.volume = targetVolume; // ensure exact restore
    }
}
