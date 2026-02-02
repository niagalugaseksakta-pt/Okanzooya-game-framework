using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public float followSpeed = 5f;
    public Vector2 deadZone = new Vector2(2f, 1f);
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Optional World Limits")]
    public bool useLimits = false;
    public Vector2 minLimit;
    public Vector2 maxLimit;

    private ScreenOrientation lastOrientation;

    void Start()
    {
        lastOrientation = Screen.orientation;
    }

    void Update()
    {
        if (Screen.orientation != lastOrientation)
        {
            lastOrientation = Screen.orientation;
            SnapToTarget();
        }
    }

    private void SnapToTarget()
    {
        if (target == null) return;
        Vector3 newPos = target.position + offset;
        transform.position = new Vector3(newPos.x, newPos.y, offset.z);
    }


    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 camPos = transform.position;
        Vector3 targetPos = target.position + offset;

        float xDiff = targetPos.x - camPos.x;
        float yDiff = targetPos.y - camPos.y;

        if (Mathf.Abs(xDiff) > deadZone.x)
            camPos.x = Mathf.Lerp(camPos.x, targetPos.x - Mathf.Sign(xDiff) * deadZone.x, Time.deltaTime * followSpeed);

        if (Mathf.Abs(yDiff) > deadZone.y)
            camPos.y = Mathf.Lerp(camPos.y, targetPos.y - Mathf.Sign(yDiff) * deadZone.y, Time.deltaTime * followSpeed);

        if (useLimits)
        {
            camPos.x = Mathf.Clamp(camPos.x, minLimit.x, maxLimit.x);
            camPos.y = Mathf.Clamp(camPos.y, minLimit.y, maxLimit.y);
        }

        transform.position = new Vector3(camPos.x, camPos.y, offset.z);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(target.position + offset, new Vector3(deadZone.x * 2, deadZone.y * 2, 0));
    }
#endif
}
