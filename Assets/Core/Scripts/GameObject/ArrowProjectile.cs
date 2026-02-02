using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 4f;
    public int damage = 10;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // example: damage the enemy if it has a Health component
        //var health = other.GetComponent<EnemyHealth>();
        //if (health != null)
        //{
        //    health.TakeDamage(damage);
        //}
        //Destroy(gameObject);
    }
}
