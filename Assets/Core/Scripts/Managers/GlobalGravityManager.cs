using UnityEngine;

[ExecuteAlways]
public class GlobalGravityManager : MonoBehaviour
{
    [Header("🌍 Global Gravity Settings")]
    public Vector2 globalGravity = new Vector2(0, -9.81f);
    [Range(0f, 2f)] public float gravityMultiplier = 1f;

    [Header("⚡ Apply Automatically")]
    public bool applyOnStart = true;
    public bool continuousUpdate = false;

    private void Awake()
    {
        // Only call DontDestroyOnLoad at runtime (when the application is playing).
        // This avoids invoking DontDestroyOnLoad while in the editor (edit mode),
        // which would throw an InvalidOperationException.
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        if (applyOnStart)
            ApplyGlobalGravity();
    }

    private void Update()
    {
        if (continuousUpdate)
            ApplyGlobalGravity();
    }

    [ContextMenu("Apply Global Gravity Now")]
    public void ApplyGlobalGravity()
    {
        Physics2D.gravity = globalGravity * gravityMultiplier;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.SceneView.RepaintAll();
#endif
    }
}
