using UnityEngine;

public class SwingingObject : EntityObstacleBase
{
    [Header("Swing Settings")]
    public float startAngle = 30f;
    public float maxAngle = -90f;
    public float speed = 2f;

    private bool swingingToMax = true; // start going from +30° → maxAngle
    public float currentAngle = 30f;

    void Start()
    {
        currentAngle = startAngle;
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

    }

    void Update()
    {
        float target = swingingToMax ? maxAngle : startAngle;
        currentAngle = Mathf.MoveTowards(currentAngle, target, speed * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

        // --- When reached max angle ---
        if (swingingToMax && Mathf.Approximately(currentAngle, maxAngle))
        {
            swingingToMax = false; // now swing back to startAngle
        }

        // --- When reached start angle (for loop) ---
        if (!swingingToMax && Mathf.Approximately(currentAngle, startAngle))
        {
            swingingToMax = true;  // swing again toward max
        }

    }

}
