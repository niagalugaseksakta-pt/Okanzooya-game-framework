using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MusicZone : MonoBehaviour
{
    public AudioClip zoneMusic;     // The music that should play in this zone
    public bool loop = true;
    private AudioSource musicSource;

    private void Start()
    {
        // Get the central music player (singleton or tagged)
        GameObject musicManager = GameObject.FindWithTag("MusicManager");
        if (musicManager != null)
            musicSource = musicManager.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoneMusic != null)
        {
            PlayZoneMusic();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StopZoneMusic();
        }
    }

    private void PlayZoneMusic()
    {
        if (musicSource.isPlaying)
            StartCoroutine(AudioFader.FadeOut(musicSource, 1f));

        StartCoroutine(AudioFader.FadeIn(musicSource, zoneMusic, 1f));
    }

    private void StopZoneMusic()
    {
        StartCoroutine(AudioFader.FadeOut(musicSource, 1f));
    }
}
