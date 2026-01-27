using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    private void OnEnable()
    {
        // Reset any state if needed
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Optional: Disable if too far away to clean up if not pooled/destroyed
        if (transform.position.z > 1000f) // Arbitrary safety net
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Obstacle obstacle = other.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.TakeDamage(damage);
            }
            
            // Disable bullet
            gameObject.SetActive(false);
        }
    }
}
