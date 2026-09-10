using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Asosiy in-game HUD boshqaruvchisi — coin, score, power-up USE tugmasi.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Coin Display")]
    public TextMeshProUGUI coinText;
    public Transform coinPopupParent;       // Popup uchun Canvas parent
    public GameObject coinPopupPrefab;      // TextMeshProUGUI bilan prefab

    [Header("Score Display")]
    public TextMeshProUGUI scoreText;

    private int displayedCoins = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            displayedCoins = GameManager.Instance.TotalCoins;
            if (coinText != null)
                coinText.text = displayedCoins.ToString();
        }
    }

    private void Update()
    {
        // Space yoki Q tugmasi — power-up ishlatish
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Q))
        {
            if (PowerUpInventory.Instance != null)
                PowerUpInventory.Instance.UseFirstSlot();
        }
    }

    // --- COIN POPUP ---

    /// <summary>
    /// Coin qo'shilganda "+N" popup ko'rsatish.
    /// Obstacle yo'q qilingan joyda paydo bo'ladi.
    /// </summary>
    public void ShowCoinPopup(int amount, Vector3 worldPosition)
    {
        if (coinPopupPrefab == null || coinPopupParent == null) return;

        // World -> Screen pozitsiya
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);

        // Prefab yaratish
        GameObject popup = Instantiate(coinPopupPrefab, coinPopupParent);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.position = screenPos;

        TextMeshProUGUI text = popup.GetComponent<TextMeshProUGUI>();
        if (text == null) text = popup.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = $"+{amount}";
            text.color = new Color(0.98f, 0.75f, 0.14f, 1f); // Oltin rang

            // Animatsiya: yuqoriga ko'tarilish + yo'qolish
            Sequence seq = DOTween.Sequence();
            seq.Append(popupRect.DOAnchorPosY(popupRect.anchoredPosition.y + 80f, 0.8f)
                .SetEase(Ease.OutQuad));
            seq.Join(text.DOFade(0f, 0.8f).SetEase(Ease.InQuad));
            seq.OnComplete(() => Destroy(popup));
        }
        else
        {
            Destroy(popup);
        }
    }

    // --- COIN DISPLAY ---

    /// <summary>
    /// Coin ko'rsatishni yangilash — DOTween counter bilan.
    /// </summary>
    public void UpdateCoinDisplay(int newTotal)
    {
        if (coinText == null) return;

        int from = displayedCoins;
        displayedCoins = newTotal;

        DOVirtual.Int(from, newTotal, 0.4f, value =>
        {
            coinText.text = value.ToString();
        });
    }

    // --- SCORE DISPLAY ---

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score}m";
    }
}
