using UnityEngine;

public class MoveUpDownRB : MonoBehaviour
{
    public float amplitude = 1f;
    public float speed = 2f;

    private Vector3 startPos;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        float y = Mathf.Sin(Time.time * speed) * amplitude;
        rb.MovePosition(startPos + new Vector3(0, y, 0));
    }
}
