using UnityEngine;

[ExecuteAlways]
public class GlobalAutoOrientation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool enableAutoOrientation = true;
    public bool autoRotateCamera = true;
    public bool autoRotateRoot = false;
    public Transform rotationRoot;
    public bool smoothRotation = true;
    public float rotationSpeed = 5f;

    private Camera cam;
    private bool isPortrait;
    private Quaternion targetRotation;

    void Awake()
    {
        if (enableAutoOrientation)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        cam = Camera.main;
        UpdateOrientation(true);
    }

    void Update()
    {
        bool currentPortrait = Screen.height >= Screen.width;

        if (currentPortrait != isPortrait)
        {
            UpdateOrientation(true);
        }

        if (smoothRotation)
        {
            if (autoRotateCamera && cam != null)
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (autoRotateRoot && rotationRoot != null)
                rotationRoot.rotation = Quaternion.Lerp(rotationRoot.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void UpdateOrientation(bool orientationChanged)
    {
        isPortrait = Screen.height >= Screen.width;
        targetRotation = isPortrait ? Quaternion.identity : Quaternion.Euler(0, 0, -90f);

        if (orientationChanged)
            Debug.Log($"[GlobalAutoOrientation] Orientation changed → {(isPortrait ? "Portrait" : "Landscape")}");
    }
}
