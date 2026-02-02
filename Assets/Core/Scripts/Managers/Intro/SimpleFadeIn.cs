using UnityEngine;
using UnityEngine.UI;

public class SimpleFadeIn : MonoBehaviour
{
    [Header("Fade Settings")]
    public float duration = 1f;

    private SpriteRenderer sprite;
    private Image image;

    private float timer = 0f;
    private bool isPlaying = false;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();

        SetAlpha(0f); // start fully invisible
    }

    void OnEnable()
    {
        Play();
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        SetAlpha(t);

        if (t >= 1f)
            isPlaying = false;
    }

    public void Play()
    {
        timer = 0f;
        isPlaying = true;
        SetAlpha(0f);
    }

    private void SetAlpha(float a)
    {
        if (sprite)
        {
            var c = sprite.color;
            c.a = a;
            sprite.color = c;
        }

        if (image)
        {
            var c = image.color;
            c.a = a;
            image.color = c;
        }
    }
}
