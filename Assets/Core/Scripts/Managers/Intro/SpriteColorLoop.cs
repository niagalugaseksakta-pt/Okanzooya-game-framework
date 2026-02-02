using UnityEngine;

public class SpriteColorLoop : MonoBehaviour
{
    [Header("Target Sprite")]
    public SpriteRenderer sprite;

    [Header("Loop Settings")]
    public float duration = 1f;   // full cycle time

    // Start (#9F8989) to End (#736363)
    private Color colorA = new Color32(0x9F, 0x89, 0x89, 0xFF);
    private Color colorB = new Color32(0x73, 0x63, 0x63, 0xFF);

    private void Reset()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!sprite) return;

        // Normalized oscillation between 0 and 1
        float t = Mathf.PingPong(Time.time / duration, 1f);

        // Smooth gradation
        sprite.color = Color.Lerp(colorA, colorB, t);
    }
}
