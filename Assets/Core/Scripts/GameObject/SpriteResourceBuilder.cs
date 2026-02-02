using System.Collections.Generic;
using UnityEngine;

//taruh di loading page
public class SpriteResourceBuilder : MonoBehaviour
{
    [Header("Parent container for generated images")]
    [SerializeField] private GameObject imageContainer;

    [Header("Folder inside Resources (e.g. 'Images' or 'Sprites/UI')")]
    [SerializeField] private string imageFolder = "Images";

    [Header("Batch load settings")]
    [SerializeField] private int batchSize = 20;

    // Automatically populated with all file names (no extension)
    [SerializeField] private List<string> imagePaths = new List<string>();

    // Public list of renderers for external access
    public List<SpriteRenderer> renderers = new List<SpriteRenderer>();

    private int currentIndex = 0;
    private bool isInitialized = false;

    void Start()
    {
        if (imageContainer == null)
        {
            imageContainer = new GameObject("ImageContainer");
            imageContainer.transform.SetParent(transform);
        }

        // Load all sprites from the specified folder
        Sprite[] sprites = Resources.LoadAll<Sprite>(imageFolder);
        imagePaths.Clear();

        foreach (var sprite in sprites)
        {
            if (sprite != null)
                imagePaths.Add($"{imageFolder}/{sprite.name}");
        }

        isInitialized = true;
        //  LoadNextBatch();
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Optional: press space to load next batch manually
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextBatch();
        }
    }

    void LoadNextBatch()
    {
        int end = Mathf.Min(currentIndex + batchSize, imagePaths.Count);

        for (int i = currentIndex; i < end; i++)
        {
            string path = imagePaths[i];
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                GameObject imgObj = new GameObject(sprite.name);
                imgObj.transform.SetParent(imageContainer.transform);
                var renderer = imgObj.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderers.Add(renderer);
            }
        }

        currentIndex = end;
    }
}
