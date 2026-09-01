using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float slowSpeed = 4f; // Slower speed when obstacle is near
    public float detectionDistance = 5f; // How far to look ahead
    public float laneWidth = 1.5f;
    public float laneChangeDuration = 0.2f;
    
    private int currentLane = 0; // -1: Left, 0: Middle, 1: Right
    private float currentSpeed;

    [Header("Shooting Settings")]
    public Transform bulletSpawnPoint;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    private Rigidbody rb;

    [Header("Power-Up States")]
    private bool hasShield = false;
    private bool hasSpeedBoost = false;
    private bool hasDoubleCoin = false;
    private float baseForwardSpeed; // Speed boost uchun asl tezlik

    [Header("Swipe Settings")]
    private Vector2 touchStartPos;
    private float swipeThreshold = 50f; // Minimal swipe masofa (piksel)
    private bool isSwiping = false;

    private void Start()
    {
        // Upgrade'dan qiymatlarni olish
        if (UpgradeManager.Instance != null)
        {
            forwardSpeed = UpgradeManager.Instance.GetValue("speed");
            fireRate = UpgradeManager.Instance.GetValue("fireRate");
        }

        currentSpeed = forwardSpeed;
        baseForwardSpeed = forwardSpeed; // Speed boost uchun asl tezlik
        
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Fixes jitter by disabling physics forces affecting transform
        }
    }

    private void Update()
    {
        // 0. Detect Obstacles Forward
        DetectObstacles();

        // 1. Move Forward
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // 2. Handle Input (Mouse/Touch or Keyboard)
        HandleInput();

        // 3. Auto Shoot
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void DetectObstacles()
    {
        // Raycast forward from current position
        RaycastHit hit;
        // We cast a bit above ground to hit the obstacle center
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(origin, Vector3.forward, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                // Slow down
                currentSpeed = Mathf.Lerp(currentSpeed, slowSpeed, Time.deltaTime * 5f);
            }
            else
            {
                // Resume speed
                currentSpeed = Mathf.Lerp(currentSpeed, forwardSpeed, Time.deltaTime * 2f);
            }
        }
        else
        {
            // Nothing strictly in front, resume speed
            currentSpeed = Mathf.Lerp(currentSpeed, forwardSpeed, Time.deltaTime * 2f);
        }
    }

    private void HandleInput()
    {
        // Keyboard (for editor testing)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            ChangeLane(1);
        }

        // Swipe detection (mobil uchun)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isSwiping = true;
                    break;
                    
                case TouchPhase.Ended:
                    if (isSwiping)
                    {
                        Vector2 swipeDelta = touch.position - touchStartPos;
                        if (Mathf.Abs(swipeDelta.x) > swipeThreshold)
                        {
                            ChangeLane(swipeDelta.x > 0 ? 1 : -1);
                        }
                        isSwiping = false;
                    }
                    break;
                    
                case TouchPhase.Canceled:
                    isSwiping = false;
                    break;
            }
        }

        // Mouse fallback (Editor'da test uchun — faqat touch yo'q bo'lganda)
        #if UNITY_EDITOR
        if (Input.touchCount == 0 && Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x < Screen.width / 2)
                ChangeLane(-1);
            else
                ChangeLane(1);
        }
        #endif
    }

    private void ChangeLane(int direction)
    {
        // Direction is -1 (Left) or 1 (Right)
        
        int targetLane = currentLane + direction;
        
        // Clamping between -1 and 1
        if (targetLane < -1 || targetLane > 1)
            return;

        currentLane = targetLane;
        
        // Calculate X position
        float targetX = currentLane * laneWidth;
        
        // DOTween the X position
        transform.DOMoveX(targetX, laneChangeDuration).SetEase(Ease.OutQuad);
    }

    private void Shoot()
    {
        // Use Object Pooling
        ObjectPooler.Instance.SpawnFromPool("Bullet", bulletSpawnPoint.position, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (hasShield)
            {
                hasShield = false; // Shield yeyiladi
                // To'siqni yo'q qilish
                Obstacle obstacle = other.GetComponent<Obstacle>();
                if (obstacle != null)
                    obstacle.TakeDamage(999); // Shield bilan to'siqni darhol buzish
                Debug.Log("Shield ishlatildi!");
            }
            else
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    // === POWER-UP TIZIMI ===

    /// <summary>
    /// Power-up ni faollashtirish
    /// </summary>
    public void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                StopCoroutine(nameof(ShieldRoutine)); // Agar oldingi davom etayotgan bo'lsa
                StartCoroutine(ShieldRoutine(5f));
                Debug.Log("🛡️ Shield faollashtirildi!");
                break;
            case PowerUpType.SpeedBoost:
                StopCoroutine(nameof(SpeedBoostRoutine));
                StartCoroutine(SpeedBoostRoutine(4f));
                Debug.Log("⚡ Speed Boost faollashtirildi!");
                break;
            case PowerUpType.DoubleCoin:
                StopCoroutine(nameof(DoubleCoinRoutine));
                StartCoroutine(DoubleCoinRoutine(8f));
                Debug.Log("💰 Double Coin faollashtirildi!");
                break;
        }
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        hasShield = true;
        yield return new WaitForSeconds(duration);
        hasShield = false;
        Debug.Log("🛡️ Shield tugadi!");
    }

    private IEnumerator SpeedBoostRoutine(float duration)
    {
        hasSpeedBoost = true;
        forwardSpeed = baseForwardSpeed * 2f; // 2x tezlik
        yield return new WaitForSeconds(duration);
        forwardSpeed = baseForwardSpeed;
        hasSpeedBoost = false;
        Debug.Log("⚡ Speed Boost tugadi!");
    }

    private IEnumerator DoubleCoinRoutine(float duration)
    {
        hasDoubleCoin = true;
        yield return new WaitForSeconds(duration);
        hasDoubleCoin = false;
        Debug.Log("💰 Double Coin tugadi!");
    }

    /// <summary>
    /// Obstacle.cs da DoubleCoin holatini tekshirish uchun
    /// </summary>
    public bool HasDoubleCoin => hasDoubleCoin;
}
