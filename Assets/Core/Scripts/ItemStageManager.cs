using UnityEngine;

public class ItemStageManager : MonoBehaviour
{
    public SpriteRenderer[] spriteRenderers;
    private SpriteRenderer childSprites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        childSprites = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (isMatch) {
        // Pick random index from 0 to 3 (inclusive)
        // int randomIndex = UnityEngine.Random.Range(0, spriteRenderers.Length);

        // Copy the sprite from the selected SpriteRenderer
        // childSprites.sprite = spriteRenderers[randomIndex].sprite;

        //}
    }
}
