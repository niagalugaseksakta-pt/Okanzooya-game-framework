using UnityEngine;

/// <summary>
/// Smoothly follows a target (e.g., Player) with optional offset, damping, and bounds.
/// Attach this script to the main camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Usually your player

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Header("Optional Look Ahead")]
    public float lookAheadDistance = 2f;
    public float lookAheadSpeed = 4f;

    [Header("Optional Camera Bounds")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 currentVelocity;
    private Vector3 lookAheadOffset;

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate look-ahead based on target's velocity (if Rigidbody2D exists)
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float xVelocity = rb.linearVelocity.x;
            float lookAheadX = Mathf.Lerp(lookAheadOffset.x, xVelocity * lookAheadDistance, Time.deltaTime * lookAheadSpeed);
            lookAheadOffset = new Vector3(lookAheadX, 0f, 0f);
        }

        // Target position with offset and look ahead
        Vector3 desiredPosition = target.position + offset + lookAheadOffset;

        // Apply bounds if enabled
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        // Smooth follow
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed);
        transform.position = smoothedPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y, 0), new Vector3(minBounds.x, maxBounds.y, 0));
        Gizmos.DrawLine(new Vector3(minBounds.x, maxBounds.y, 0), new Vector3(maxBounds.x, maxBounds.y, 0));
        Gizmos.DrawLine(new Vector3(maxBounds.x, maxBounds.y, 0), new Vector3(maxBounds.x, minBounds.y, 0));
        Gizmos.DrawLine(new Vector3(maxBounds.x, minBounds.y, 0), new Vector3(minBounds.x, minBounds.y, 0));
    }
}
