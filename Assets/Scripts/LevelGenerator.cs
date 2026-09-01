using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Ground Settings")]
    public Transform playerTransform;
    public int spawnDistance = 15; // Increased to prevent pop-in
    public float groundLength = 10f; // As per user: z = 10
    
    // We keep track of active grounds to recycled them
    private List<GameObject> activeGrounds = new List<GameObject>();
    private float lastGroundZ = 0f; // Track where the next ground connects

    [Header("Obstacle Settings")]
    [Range(0f, 1f)] public float obstacleSpawnChance = 0.5f;

    [Header("Power-Up Settings")]
    [Range(3, 8)] public int powerUpInterval = 5;  // Har nechta segmentda 1 ta power-up
    private int segmentsSinceLastPowerUp = 0;

    // To'siq joylashuvi pattern'lari — KAMIDA 1 LANE DOIM BO'SH
    private static readonly int[][] obstaclePatterns = new int[][]
    {
        new int[] { 1, 0, 0 },  // Faqat chap
        new int[] { 0, 1, 0 },  // Faqat o'rta
        new int[] { 0, 0, 1 },  // Faqat o'ng
        new int[] { 1, 1, 0 },  // Chap + O'rta
        new int[] { 1, 0, 1 },  // Chap + O'ng
        new int[] { 0, 1, 1 },  // O'rta + O'ng
    };

    // Lane X pozitsiyalari
    private static readonly float[] laneX = { -1.5f, 0f, 1.5f };

    // Power-up turlari (ObjectPooler tag'lari bilan mos)
    private static readonly string[] powerUpTags = { "Shield", "SpeedBoost", "DoubleCoin" };

    private void Start()
    {
        // Initial set of grounds
        for (int i = 0; i < spawnDistance; i++)
        {
            SpawnGround();
        }
    }

    private void Update()
    {
        // Check if player moved far enough to spawn new ground
        if (playerTransform.position.z - 15 > (activeGrounds[0].transform.position.z + groundLength / 2))
        {
            RecycleGround();
        }
    }

    public void SpawnGround()
    {
        // 1. Get ground from pool
        GameObject ground = ObjectPooler.Instance.SpawnFromPool("Ground", Vector3.forward * lastGroundZ, Quaternion.identity);
        
        // 2. Spawn obstacles (skip first couple segments for start zone)
        if (lastGroundZ > 10)
        {
            SpawnObstacles(ground.transform);
        }

        activeGrounds.Add(ground);
        lastGroundZ += groundLength;
    }

    private void RecycleGround()
    {
        GameObject oldGround = activeGrounds[0];
        activeGrounds.RemoveAt(0);
        oldGround.SetActive(false); // Return to pool implicitly by disabling
        
        // Spawn a new one at the front
        SpawnGround();
    }

    private void SpawnObstacles(Transform parentGround)
    {
        // Difficulty ga qarab spawn ehtimolini oshirish
        float adjustedChance = obstacleSpawnChance;
        if (GameManager.Instance != null)
            adjustedChance = Mathf.Min(0.9f, obstacleSpawnChance * GameManager.Instance.difficultyMultiplier);

        if (Random.value > adjustedChance) return; // Sometimes empty ground

        // Pattern-based spawn — kamida 1 lane doim bo'sh
        int patternIndex = Random.Range(0, obstaclePatterns.Length);
        int[] pattern = obstaclePatterns[patternIndex];

        float zOffset = 0f;

        // Bo'sh lane'larni kuzatish (power-up joylash uchun)
        List<int> emptyLanes = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            if (pattern[i] == 1)
            {
                Vector3 spawnPos = new Vector3(laneX[i], 0.8f, parentGround.position.z + zOffset);
                ObjectPooler.Instance.SpawnFromPool("Obstacle", spawnPos, Quaternion.identity);
            }
            else
            {
                emptyLanes.Add(i);
            }
        }

        // Power-up spawn — har N ta segmentda 1 ta
        segmentsSinceLastPowerUp++;
        if (segmentsSinceLastPowerUp >= powerUpInterval && emptyLanes.Count > 0)
        {
            SpawnPowerUp(parentGround, emptyLanes, zOffset);
            segmentsSinceLastPowerUp = 0;
        }
    }

    private void SpawnPowerUp(Transform parentGround, List<int> emptyLanes, float zOffset)
    {
        // Tasodifiy bo'sh lane'ni tanlash
        int laneIndex = emptyLanes[Random.Range(0, emptyLanes.Count)];
        
        // Tasodifiy power-up turini tanlash
        string powerUpTag = powerUpTags[Random.Range(0, powerUpTags.Length)];
        
        Vector3 spawnPos = new Vector3(laneX[laneIndex], 1.0f, parentGround.position.z + zOffset);
        
        GameObject powerUp = ObjectPooler.Instance.SpawnFromPool(powerUpTag, spawnPos, Quaternion.identity);
        if (powerUp == null)
        {
            Debug.LogWarning($"Power-up pool '{powerUpTag}' topilmadi yoki bo'sh. ObjectPooler'ga qo'shing!");
        }
    }
}
