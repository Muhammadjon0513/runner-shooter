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
        // O'yinchi bilan to'qnashganda — inventarga qo'shish
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            // Inventarga qo'shishga urinish
            if (PowerUpInventory.Instance != null && PowerUpInventory.Instance.AddPowerUp(type))
            {
                gameObject.SetActive(false); // Pool'ga qaytish
            }
            // Agar inventar to'la — power-up yo'lda qoladi (yig'ilmaydi)
        }
    }
}
