using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Coin System")]
    public int TotalCoins = 0;      // Barcha sessiyalardagi jami coin (PlayerPrefs)
    public int SessionCoins = 0;    // Hozirgi sessiya coinlari

    [Header("Score System")]
    public int Score = 0;           // Distance-based score (hozirgi sessiya)
    public int BestScore = 0;      // Eng yuqori natija (PlayerPrefs)
    private Transform playerTransform;

    [Header("Difficulty")]
    public float difficultyMultiplier = 1f;
    public float difficultyIncreaseInterval = 30f; // Har 30 sekundda
    public float difficultyIncreaseAmount = 0.1f;  // 10% oshadi
    private float nextDifficultyTime;

    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI scoreText;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadData();
    }

    private void Start()
    {
        // O'yinchi Transform'ini topish
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        nextDifficultyTime = Time.time + difficultyIncreaseInterval;
        UpdateUI();
    }

    private void Update()
    {
        if (isGameOver) return;

        // Distance-based score
        if (playerTransform != null)
        {
            Score = Mathf.FloorToInt(playerTransform.position.z);
            if (scoreText != null)
                scoreText.text = Score.ToString();
            if (HUDManager.Instance != null)
                HUDManager.Instance.UpdateScoreDisplay(Score);
        }

        // Difficulty scaling — har 30 sekundda 10% oshadi
        if (Time.time >= nextDifficultyTime)
        {
            difficultyMultiplier += difficultyIncreaseAmount;
            nextDifficultyTime = Time.time + difficultyIncreaseInterval;
            Debug.Log($"Difficulty increased to {difficultyMultiplier:F1}x");
        }
    }

    public void AddCoin(int amount)
    {
        SessionCoins += amount;
        TotalCoins += amount;
        UpdateUI();

        // HUD popup va counter
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateCoinDisplay(TotalCoins);
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = TotalCoins.ToString();
    }

    /// <summary>
    /// Coin sarflash (upgrade sotib olish uchun)
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (TotalCoins >= amount)
        {
            TotalCoins -= amount;
            UpdateUI();
            SaveData();
            return true;
        }
        return false;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"Game Over! Score: {Score}, Coins: {SessionCoins}");

        // Eng yuqori natijani yangilash
        bool isNewRecord = Score > BestScore;
        if (isNewRecord)
        {
            BestScore = Score;
            Debug.Log($"New Best Score: {BestScore}");
        }

        SaveData();

        // Power-up inventarni tozalash
        if (PowerUpInventory.Instance != null)
            PowerUpInventory.Instance.ClearInventory();

        // GameOver UI panelini ko'rsatish
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.ShowGameOver(Score, SessionCoins, BestScore, isNewRecord);
        }
        else
        {
            // Fallback — agar UI yo'q bo'lsa darhol restart
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    // --- PlayerPrefs bilan saqlash/yuklash ---

    public void SaveData()
    {
        PlayerPrefs.SetInt("TotalCoins", TotalCoins);
        PlayerPrefs.SetInt("BestScore", BestScore);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        TotalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        BestScore = PlayerPrefs.GetInt("BestScore", 0);
    }
}
