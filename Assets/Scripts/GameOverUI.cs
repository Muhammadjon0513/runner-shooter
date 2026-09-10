using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

/// <summary>
/// Game Over paneli — natijalar, restart, upgrade do'koniga o'tish.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("Panel")]
    public GameObject gameOverPanel;
    public CanvasGroup panelCanvasGroup;
    public RectTransform panelRect;

    [Header("Natija Textlari")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI bestScoreText;
    public GameObject newRecordBadge;     // "YANGI REKORD!" yozuvi

    [Header("Tugmalar")]
    public Button restartButton;
    public Button shopButton;

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
        // Tugmalarga listener
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (shopButton != null)
            shopButton.onClick.AddListener(OnShopClicked);

        // Boshlang'ichda yashirish
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (newRecordBadge != null)
            newRecordBadge.SetActive(false);
    }

    /// <summary>
    /// Game Over panelini ko'rsatish — animatsiyalar bilan.
    /// </summary>
    public void ShowGameOver(int score, int sessionCoins, int bestScore, bool isNewRecord)
    {
        if (gameOverPanel == null) return;

        Time.timeScale = 0f; // O'yinni to'xtatish
        gameOverPanel.SetActive(true);

        // Boshlang'ich holat
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;
        if (panelRect != null)
            panelRect.anchoredPosition = new Vector2(0, -800f);

        // Panel animatsiyasi — pastdan yuqoriga slide + fade
        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (panelRect != null)
            seq.Append(panelRect.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutBack));
        if (panelCanvasGroup != null)
            seq.Join(panelCanvasGroup.DOFade(1f, 0.3f));

        // Counter animatsiyalar — raqamlar 0 dan qiymatgacha
        seq.AppendCallback(() =>
        {
            // Score counter
            if (scoreText != null)
            {
                DOVirtual.Int(0, score, 1f, value =>
                {
                    scoreText.text = $"{value}m";
                }).SetUpdate(true);
            }

            // Coin counter
            if (coinsText != null)
            {
                DOVirtual.Int(0, sessionCoins, 0.8f, value =>
                {
                    coinsText.text = $"+{value}";
                }).SetUpdate(true);
            }

            // Best score
            if (bestScoreText != null)
                bestScoreText.text = $"Best: {bestScore}m";
        });

        // Yangi rekord badge
        if (isNewRecord && newRecordBadge != null)
        {
            seq.AppendInterval(0.8f);
            seq.AppendCallback(() =>
            {
                newRecordBadge.SetActive(true);
                newRecordBadge.transform.localScale = Vector3.zero;
                newRecordBadge.transform.DOScale(1f, 0.4f)
                    .SetEase(Ease.OutElastic)
                    .SetUpdate(true);
            });
        }

        // Tugmalar animatsiyasi
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() =>
        {
            if (restartButton != null)
            {
                restartButton.transform.localScale = Vector3.zero;
                restartButton.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
            if (shopButton != null)
            {
                shopButton.transform.localScale = Vector3.zero;
                shopButton.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.1f)
                    .SetUpdate(true);
            }
        });
    }

    public void HideGameOver()
    {
        if (gameOverPanel == null) return;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (panelCanvasGroup != null)
            seq.Append(panelCanvasGroup.DOFade(0f, 0.2f));
        if (panelRect != null)
            seq.Join(panelRect.DOAnchorPosY(-800f, 0.3f).SetEase(Ease.InBack));

        seq.OnComplete(() =>
        {
            gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
        });
    }

    // --- TUGMA HANDLERLARI ---

    private void OnRestartClicked()
    {
        DOTween.KillAll();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnShopClicked()
    {
        HideGameOver();

        // Upgrade do'konini ochish
        UpgradeShopUI shopUI = FindObjectOfType<UpgradeShopUI>();
        if (shopUI != null)
            shopUI.ShowShop();
    }
}
