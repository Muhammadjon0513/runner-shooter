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
        // We delete ground behind the player
        
        // Assuming the player starts at 0, and grounds are 10m long.
        // If player is at Z=20, we probably don't need the ground at Z=0 anymore.
        
        if (playerTransform.position.z - 15 > (activeGrounds[0].transform.position.z + groundLength / 2))
        {
            RecycleGround();
        }
    }

    public void SpawnGround()
    {
        // 1. Get ground from pool
        GameObject ground = ObjectPooler.Instance.SpawnFromPool("Ground", Vector3.forward * lastGroundZ, Quaternion.identity);
        
        // 2. Randomly spawn obstacles on this ground 
        // We don't spawn on the VERY first ground usually (to give player start time), 
        // but for simplicity let's just use strict logic:
        if (lastGroundZ > 10) // Skip first couple segments
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
        // User requested: "obstacles should be spawned in the same z position"
        // This implies a ROW of obstacles.
        // Lanes are X: -1.5, 0, 1.5.
        
        if (Random.value > obstacleSpawnChance) return; // Sometimes empty ground

        // Pick a random configuration of the 3 lanes.
        // e.g. [1, 0, 1] means Left and Right have obstacles.
        // [1, 1, 1] means wall (impossible? maybe destroyable)
        
        // Let's spawn 1 to 3 obstacles in a row at the center of the ground segment (Z offset)
        float zOffset = 0; // Center of the 10 unit long ground relative to its pivot? 
                           // If pivot is center, zOffset 0 is fine.
                           
        // Lanes
        float[] laneX = { -1.5f, 0f, 1.5f };
        
        // Decision: Spawn row.
        foreach (float x in laneX)
        {
            // 70% chance to spawn in a given slot? Or explicit patterns?
            // "so now the tank moves forward... automatic"
            // Let's just randomize each lane independently for now
            if (Random.value > 0.3f) 
            {
                Vector3 spawnPos = new Vector3(x, 0.8f, parentGround.position.z + zOffset);
                // Note: We spawn in WORLD space, but aligned with ground Z.
                ObjectPooler.Instance.SpawnFromPool("Obstacle", spawnPos, Quaternion.identity);
            }
        }
    }
}
