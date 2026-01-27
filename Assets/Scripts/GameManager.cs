using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int Coins = 0;
    
    // UI References (assign in inspector)
    public TextMeshProUGUI coinText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCoin(int amount)
    {
        Coins += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = Coins.ToString();
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        // Simple restart for now, or show menu
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
