using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [System.Serializable]
    public class UpgradeData
    {
        public string id;           // "damage", "fireRate", "speed", "coinBonus"
        public string displayName;  // UI uchun nom
        public int maxLevel;        // Maksimal daraja (5)
        public int basePrice;       // Boshlang'ich narx
        public float priceMultiplier; // Narx ko'paytiruvchi (2 = har daraja 2x)
        public float[] values;      // Har daraja uchun qiymat [0] = default

        [HideInInspector]
        public int currentLevel;    // Hozirgi daraja (0 = sotib olinmagan)
    }

    [Header("Upgrade Configurations")]
    public List<UpgradeData> upgrades = new List<UpgradeData>();

    private Dictionary<string, UpgradeData> upgradeMap;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildMap();
        LoadUpgrades();
    }

    private void BuildMap()
    {
        upgradeMap = new Dictionary<string, UpgradeData>();
        foreach (var upgrade in upgrades)
        {
            upgradeMap[upgrade.id] = upgrade;
        }
    }

    /// <summary>
    /// Upgrade'ning hozirgi darajasini qaytaradi
    /// </summary>
    public int GetLevel(string id)
    {
        if (upgradeMap.TryGetValue(id, out UpgradeData data))
            return data.currentLevel;
        
        Debug.LogWarning($"Upgrade '{id}' topilmadi!");
        return 0;
    }

    /// <summary>
    /// Upgrade'ning hozirgi daraja qiymatini qaytaradi
    /// </summary>
    public float GetValue(string id)
    {
        if (upgradeMap.TryGetValue(id, out UpgradeData data))
        {
            int level = Mathf.Clamp(data.currentLevel, 0, data.values.Length - 1);
            return data.values[level];
        }

        Debug.LogWarning($"Upgrade '{id}' topilmadi!");
        return 0f;
    }

    /// <summary>
    /// Keyingi daraja uchun narxni qaytaradi
    /// </summary>
    public int GetUpgradePrice(string id)
    {
        if (upgradeMap.TryGetValue(id, out UpgradeData data))
        {
            if (data.currentLevel >= data.maxLevel)
                return -1; // Maksimal daraja

            // basePrice * priceMultiplier^currentLevel
            return Mathf.RoundToInt(data.basePrice * Mathf.Pow(data.priceMultiplier, data.currentLevel));
        }
        return -1;
    }

    /// <summary>
    /// Upgrade sotib olish imkoniyatini tekshiradi
    /// </summary>
    public bool CanAfford(string id)
    {
        int price = GetUpgradePrice(id);
        if (price < 0) return false; // Maks daraja yoki noto'g'ri id
        return GameManager.Instance.TotalCoins >= price;
    }

    /// <summary>
    /// Upgrade'ni maximal daraja ekanligini tekshiradi
    /// </summary>
    public bool IsMaxLevel(string id)
    {
        if (upgradeMap.TryGetValue(id, out UpgradeData data))
            return data.currentLevel >= data.maxLevel;
        return true;
    }

    /// <summary>
    /// Upgrade sotib olishga urinish — coin yetsa daraja oshadi
    /// </summary>
    public bool TryUpgrade(string id)
    {
        if (!upgradeMap.TryGetValue(id, out UpgradeData data))
        {
            Debug.LogWarning($"Upgrade '{id}' topilmadi!");
            return false;
        }

        if (data.currentLevel >= data.maxLevel)
        {
            Debug.Log($"Upgrade '{id}' allaqachon maksimal darajada!");
            return false;
        }

        int price = GetUpgradePrice(id);
        if (GameManager.Instance.SpendCoins(price))
        {
            data.currentLevel++;
            SaveUpgrades();
            Debug.Log($"Upgrade '{id}' -> Level {data.currentLevel} (qiymat: {GetValue(id)})");
            return true;
        }

        Debug.Log($"Coin yetarli emas! Kerak: {price}, Bor: {GameManager.Instance.TotalCoins}");
        return false;
    }

    // --- PlayerPrefs bilan saqlash/yuklash ---

    public void SaveUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            PlayerPrefs.SetInt("Upgrade_" + upgrade.id, upgrade.currentLevel);
        }
        PlayerPrefs.Save();
    }

    public void LoadUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            upgrade.currentLevel = PlayerPrefs.GetInt("Upgrade_" + upgrade.id, 0);
        }
    }

    /// <summary>
    /// Barcha upgrade'larni 0 ga tushirish (debug uchun)
    /// </summary>
    public void ResetAllUpgrades()
    {
        foreach (var upgrade in upgrades)
        {
            upgrade.currentLevel = 0;
            PlayerPrefs.DeleteKey("Upgrade_" + upgrade.id);
        }
        PlayerPrefs.Save();
        Debug.Log("Barcha upgrade'lar qayta tiklandi!");
    }
}
