using UnityEngine;

public enum PowerUpType
{
    Shield,
    SpeedBoost,
    DoubleCoin
}

public class PowerUp : MonoBehaviour
{
    public PowerUpType type;

    [Header("Visual")]
    public float rotateSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    private Vector3 startPos;

    private void OnEnable()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Aylanish
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // Yuqoriga-pastga harakat (bob)
        Vector3 pos = transform.position;
        pos.y = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        // O'yinchi bilan to'qnashganda
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ActivatePowerUp(type);
            gameObject.SetActive(false); // Pool'ga qaytish
        }
    }
}
