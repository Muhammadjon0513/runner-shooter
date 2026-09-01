using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    public float lifeTime = 3f;

    // BUG-6 Fix: Coroutine reference — xavfsiz to'xtatish uchun
    private Coroutine deactivateCoroutine;

    private void OnEnable()
    {
        // Upgrade'dan damage olish
        if (UpgradeManager.Instance != null)
            damage = (int)UpgradeManager.Instance.GetValue("damage");

        // BUG-6 Fix: Invoke o'rniga Coroutine — kompilyator xatoni ushlay oladi
        deactivateCoroutine = StartCoroutine(DeactivateAfterTime());
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }
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
