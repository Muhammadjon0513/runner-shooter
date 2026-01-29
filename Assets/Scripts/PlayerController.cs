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

    private void Start()
    {
        currentSpeed = forwardSpeed;
        
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

        // Mouse / Touch (Simple left/right side of screen center)
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x < Screen.width / 2)
                ChangeLane(-1);
            else
                ChangeLane(1);
        }
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
            GameManager.Instance.GameOver();
        }
    }
}
