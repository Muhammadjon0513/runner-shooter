using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        // BUG-5 Fix: Singleton himoyasi (GameManager kabi)
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];

        // BUG-1 Fix: Faol bo'lmagan obyektni qidirish
        for (int i = 0; i < queue.Count; i++)
        {
            GameObject obj = queue.Dequeue();
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                queue.Enqueue(obj);
                return obj;
            }
            queue.Enqueue(obj); // Faol — qaytarib qo'y
        }

        // Pool tugagan — yangi obyekt yarat (auto-expand)
        Pool poolInfo = pools.Find(p => p.tag == tag);
        if (poolInfo != null)
        {
            GameObject newObj = Instantiate(poolInfo.prefab);
            newObj.transform.position = position;
            newObj.transform.rotation = rotation;
            queue.Enqueue(newObj);
            return newObj;
        }

        Debug.LogWarning("Pool with tag " + tag + " is exhausted and has no prefab to expand.");
        return null;
    }
}
