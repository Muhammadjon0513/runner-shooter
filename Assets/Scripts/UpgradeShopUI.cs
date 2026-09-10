using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Upgrade do'kon paneli — har bir upgrade uchun kartochka, narx, progress bar.
/// </summary>
public class UpgradeShopUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject shopPanel;
    public Button startButton;          // O'yinni boshlash
    public TextMeshProUGUI totalCoinsText;

    [Header("Upgrade Cards")]
    public Transform contentParent;     // Kartochkalar uchun parent (Vertical Layout Group)
    public GameObject upgradeCardPrefab; // Kartochka prefabi

    // Har bir kartochka uchun reference
    private class UpgradeCardRef
    {
        public string upgradeId;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI currentValueText;
        public TextMeshProUGUI nextValueText;
        public Image progressFill;
        public Button buyButton;
        public GameObject maxBadge;
        public RectTransform rectTransform;
    }

    private List<UpgradeCardRef> cards = new List<UpgradeCardRef>();
    private bool isInitialized = false;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        // Boshlang'ichda yashirish
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void ShowShop()
    {
        if (shopPanel == null) return;

        Time.timeScale = 0f; // O'yinni to'xtatish
        shopPanel.SetActive(true);

        if (!isInitialized)
        {
            CreateCards();
            isInitialized = true;
        }

        RefreshCards();
        UpdateCoinsDisplay();

        // DOTween animatsiya — scale pop
        shopPanel.transform.localScale = Vector3.zero;
        shopPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void HideShop()
    {
        if (shopPanel == null) return;

        shopPanel.transform.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                shopPanel.SetActive(false);
                Time.timeScale = 1f; // O'yinni davom ettirish
            });
    }

    private void OnStartClicked()
    {
        HideShop();
    }

    // --- KARTOCHKALAR YARATISH ---

    private void CreateCards()
    {
        if (UpgradeManager.Instance == null || upgradeCardPrefab == null || contentParent == null)
            return;

        // Eski kartochkalarni tozalash
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        cards.Clear();

        foreach (var upgrade in UpgradeManager.Instance.upgrades)
        {
            GameObject cardObj = Instantiate(upgradeCardPrefab, contentParent);
            UpgradeCardRef card = new UpgradeCardRef();
            card.upgradeId = upgrade.id;
            card.rectTransform = cardObj.GetComponent<RectTransform>();

            // Child elementlarni topish (prefab ichidagi nomlar bo'yicha)
            card.nameText = cardObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            card.levelText = cardObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            card.priceText = cardObj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            card.currentValueText = cardObj.transform.Find("CurrentValueText")?.GetComponent<TextMeshProUGUI>();
            card.nextValueText = cardObj.transform.Find("NextValueText")?.GetComponent<TextMeshProUGUI>();
            card.progressFill = cardObj.transform.Find("ProgressBar/Fill")?.GetComponent<Image>();
            card.buyButton = cardObj.transform.Find("BuyButton")?.GetComponent<Button>();
            card.maxBadge = cardObj.transform.Find("MaxBadge")?.gameObject;

            // Buy tugmasiga listener
            string id = upgrade.id; // Closure uchun lokal nusxa
            if (card.buyButton != null)
                card.buyButton.onClick.AddListener(() => OnBuyClicked(id));

            cards.Add(card);
        }
    }

    // --- KARTOCHKALARNI YANGILASH ---

    public void RefreshCards()
    {
        if (UpgradeManager.Instance == null) return;

        foreach (var card in cards)
        {
            var upgrade = UpgradeManager.Instance.upgrades.Find(u => u.id == card.upgradeId);
            if (upgrade == null) continue;

            int level = upgrade.currentLevel;
            bool isMax = UpgradeManager.Instance.IsMaxLevel(card.upgradeId);
            int price = UpgradeManager.Instance.GetUpgradePrice(card.upgradeId);
            bool canAfford = UpgradeManager.Instance.CanAfford(card.upgradeId);

            // Nom
            if (card.nameText != null)
                card.nameText.text = upgrade.displayName;

            // Level
            if (card.levelText != null)
                card.levelText.text = $"Lvl {level}/{upgrade.maxLevel}";

            // Narx
            if (card.priceText != null)
            {
                if (isMax)
                    card.priceText.text = "MAX";
                else
                    card.priceText.text = $"{price} \u00a4";
            }

            // Hozirgi qiymat
            if (card.currentValueText != null)
            {
                float currentVal = UpgradeManager.Instance.GetValue(card.upgradeId);
                card.currentValueText.text = $"Hozirgi: {currentVal:F2}";
            }

            // Keyingi qiymat
            if (card.nextValueText != null)
            {
                if (!isMax && level + 1 < upgrade.values.Length)
                    card.nextValueText.text = $"Keyingi: {upgrade.values[level + 1]:F2}";
                else
                    card.nextValueText.text = "";
            }

            // Progress bar
            if (card.progressFill != null)
            {
                float target = (float)level / upgrade.maxLevel;
                card.progressFill.DOFillAmount(target, 0.3f).SetUpdate(true);
            }

            // Buy tugmasi
            if (card.buyButton != null)
            {
                card.buyButton.interactable = !isMax && canAfford;
            }

            // MAX badge
            if (card.maxBadge != null)
                card.maxBadge.SetActive(isMax);
        }
    }

    // --- SOTIB OLISH ---

    private void OnBuyClicked(string upgradeId)
    {
        if (UpgradeManager.Instance == null) return;

        if (UpgradeManager.Instance.TryUpgrade(upgradeId))
        {
            // Muvaffaqiyatli — animatsiya
            var card = cards.Find(c => c.upgradeId == upgradeId);
            if (card != null && card.rectTransform != null)
            {
                card.rectTransform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f).SetUpdate(true);
            }

            RefreshCards();
            UpdateCoinsDisplay();
        }
        else
        {
            // Coin yetmadi — shake animatsiya
            var card = cards.Find(c => c.upgradeId == upgradeId);
            if (card != null && card.priceText != null)
            {
                card.priceText.DOKill();
                card.priceText.rectTransform.DOShakePosition(0.3f, 10f, 20, 90f, false, true)
                    .SetUpdate(true);
                card.priceText.DOColor(Color.red, 0.15f).SetUpdate(true)
                    .OnComplete(() => card.priceText.DOColor(Color.white, 0.15f).SetUpdate(true));
            }
        }
    }

    private void UpdateCoinsDisplay()
    {
        if (totalCoinsText != null && GameManager.Instance != null)
            totalCoinsText.text = $"{GameManager.Instance.TotalCoins} \u00a4";
    }
}
