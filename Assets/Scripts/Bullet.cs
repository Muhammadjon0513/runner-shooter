using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    public float lifeTime = 3f;

    private void OnEnable()
    {
        // Disable after lifeTime seconds
        Invoke("Deactivate", lifeTime);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
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
