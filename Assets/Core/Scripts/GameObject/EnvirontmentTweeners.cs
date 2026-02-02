using UnityEngine;

public class EnvironmentTweeners : MonoBehaviour
{
    public enum MoveDirection { Left = -1, Right = 1 }

    [System.Serializable]
    public class Layer
    {
        [Header("Main Object (Prefab Instance)")]
        public Transform target;

        [Header("Movement Settings")]
        public MoveDirection direction = MoveDirection.Left;
        public float speed = 1f;

        [Header("Loop Settings")]
        public bool duplicate = true;
        public float gap = 0f;

        [HideInInspector] public Transform clone;
        [HideInInspector] public float width;
    }

    [Header("Configuration")]
    public Layer[] layers;
    public Camera mainCamera;

    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;

        foreach (var layer in layers)
        {
            if (layer.target == null) continue;

            // Detect width from Renderer
            var renderer = layer.target.GetComponentInChildren<Renderer>();
            layer.width = renderer ? renderer.bounds.size.x : 10f;

            // Create clone
            if (layer.duplicate && layer.clone == null)
            {
                layer.clone = Instantiate(layer.target, layer.target.parent);
                layer.clone.name = layer.target.name + "_Clone";

                float dir = (layer.direction == MoveDirection.Left) ? 1 : -1;
                layer.clone.position = layer.target.position + Vector3.right * dir * (layer.width + layer.gap);
            }
        }
    }

    void Update()
    {
        if (!mainCamera) mainCamera = Camera.main;

        foreach (var layer in layers)
        {
            if (layer.target == null) continue;

            float moveDelta = layer.speed * Time.deltaTime * (int)layer.direction;
            MoveLayer(layer, moveDelta);
        }
    }

    private void MoveLayer(Layer layer, float moveDelta)
    {
        // Move both
        layer.target.position += Vector3.right * moveDelta;
        if (layer.clone) layer.clone.position += Vector3.right * moveDelta;

        // Camera limits
        float camHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float camLeft = mainCamera.transform.position.x - camHalfWidth;
        float camRight = mainCamera.transform.position.x + camHalfWidth;

        // --- LEFT Direction
        if (layer.direction == MoveDirection.Left)
        {
            if (layer.target.position.x + layer.width / 2f < camLeft)
                ResetRight(layer, layer.clone);
            else if (layer.clone.position.x + layer.width / 2f < camLeft)
                ResetRight(layer, layer.target);
        }

        // --- RIGHT Direction
        else
        {
            if (layer.target.position.x - layer.width / 2f > camRight)
                ResetLeft(layer, layer.clone);
            else if (layer.clone.position.x - layer.width / 2f > camRight)
                ResetLeft(layer, layer.target);
        }
    }

    private void ResetRight(Layer layer, Transform reference)
    {
        float newX = reference.position.x + (layer.width + layer.gap);
        if (layer.target == reference)
            layer.clone.position = new Vector3(newX, reference.position.y, reference.position.z);
        else
            layer.target.position = new Vector3(newX, reference.position.y, reference.position.z);
    }

    private void ResetLeft(Layer layer, Transform reference)
    {
        float newX = reference.position.x - (layer.width + layer.gap);
        if (layer.target == reference)
            layer.clone.position = new Vector3(newX, reference.position.y, reference.position.z);
        else
            layer.target.position = new Vector3(newX, reference.position.y, reference.position.z);
    }
}
