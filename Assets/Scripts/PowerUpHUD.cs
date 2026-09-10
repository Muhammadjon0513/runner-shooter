using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// Blur uslubidagi power-up HUD — 3 ta slot + timer bar + flash text.
/// </summary>
public class PowerUpHUD : MonoBehaviour
{
    public static PowerUpHUD Instance;

    [Header("Slot UI")]
    public Image[] slotImages = new Image[3];      // 3 ta slot Image
    public Sprite shieldIcon;                       // Yashil qalqon
    public Sprite speedIcon;                        // Ko'k tezlik
    public Sprite doubleCoinIcon;                   // Oltin coin
    public Sprite emptySlotIcon;                    // Bo'sh slot

    [Header("Use Button")]
    public Button useButton;

    [Header("Timer Bar")]
    public Image timerBar;                          // FillAmount bilan ishlaydi
    public Image timerBarBackground;

    [Header("Flash Text")]
    public TextMeshProUGUI flashText;               // Power-up nomi

    // Rang kodlari
    private readonly Color shieldColor = new Color(0.29f, 0.85f, 0.50f);    // #4ade80
    private readonly Color speedColor = new Color(0.13f, 0.83f, 0.93f);     // #22d3ee
    private readonly Color coinColor = new Color(0.98f, 0.75f, 0.14f);      // #fbbf24
    private readonly Color emptyColor = new Color(1f, 1f, 1f, 0.2f);

    private PlayerController playerController;
    private Tween timerTween;

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
        // Player event'lariga ulash
        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.OnPowerUpActivated += OnPowerUpActivated;
            playerController.OnPowerUpExpired += OnPowerUpExpired;
        }

        // Inventar event'iga ulash
        if (PowerUpInventory.Instance != null)
            PowerUpInventory.Instance.OnInventoryChanged += UpdateSlots;

        // Use tugmasi
        if (useButton != null)
            useButton.onClick.AddListener(OnUseButtonClicked);

        // Boshlang'ich holat
        HideTimer();
        HideFlashText();
        ClearSlots();
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnPowerUpActivated -= OnPowerUpActivated;
            playerController.OnPowerUpExpired -= OnPowerUpExpired;
        }
        if (PowerUpInventory.Instance != null)
            PowerUpInventory.Instance.OnInventoryChanged -= UpdateSlots;
    }

    // --- SLOT BOSHQARUVI ---

    public void UpdateSlots(List<PowerUpType> inventory)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < inventory.Count)
            {
                // Slot to'la — ikon va rang ko'rsatish
                slotImages[i].sprite = GetIcon(inventory[i]);
                slotImages[i].color = GetColor(inventory[i]);

                // DOTween bounce animatsiya
                slotImages[i].transform.DOKill();
                slotImages[i].transform.localScale = Vector3.one;
                slotImages[i].transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
            }
            else
            {
                // Slot bo'sh
                slotImages[i].sprite = emptySlotIcon;
                slotImages[i].color = emptyColor;
            }
        }

        // Use tugmasini yoqish/o'chirish
        if (useButton != null)
            useButton.interactable = inventory.Count > 0;
    }

    private void ClearSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            slotImages[i].sprite = emptySlotIcon;
            slotImages[i].color = emptyColor;
        }
    }

    // --- TIMER BAR ---

    private void OnPowerUpActivated(PowerUpType type, float duration)
    {
        // Timer bar ko'rsatish
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(true);
            if (timerBarBackground != null)
                timerBarBackground.gameObject.SetActive(true);

            timerBar.color = GetColor(type);
            timerBar.fillAmount = 1f;

            // Silliq kamayish
            timerTween?.Kill();
            timerTween = timerBar.DOFillAmount(0f, duration)
                .SetEase(Ease.Linear)
                .OnComplete(HideTimer);
        }

        // Flash text
        ShowFlashText(GetDisplayName(type), GetColor(type));
    }

    private void OnPowerUpExpired(PowerUpType type)
    {
        HideTimer();
    }

    private void HideTimer()
    {
        if (timerBar != null)
            timerBar.gameObject.SetActive(false);
        if (timerBarBackground != null)
            timerBarBackground.gameObject.SetActive(false);
    }

    // --- FLASH TEXT ---

    private void ShowFlashText(string text, Color color)
    {
        if (flashText == null) return;

        flashText.text = text;
        flashText.color = color;
        flashText.gameObject.SetActive(true);

        // Animatsiya: paydo bo'lish → 1s turish → yo'qolish
        flashText.DOKill();
        flashText.alpha = 0f;
        DOTween.Sequence()
            .Append(flashText.DOFade(1f, 0.2f))
            .AppendInterval(1f)
            .Append(flashText.DOFade(0f, 0.3f))
            .OnComplete(HideFlashText);
    }

    private void HideFlashText()
    {
        if (flashText != null)
            flashText.gameObject.SetActive(false);
    }

    // --- USE TUGMASI ---

    private void OnUseButtonClicked()
    {
        if (PowerUpInventory.Instance != null)
            PowerUpInventory.Instance.UseFirstSlot();
    }

    // --- YORDAMCHI METODLAR ---

    private Sprite GetIcon(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield: return shieldIcon;
            case PowerUpType.SpeedBoost: return speedIcon;
            case PowerUpType.DoubleCoin: return doubleCoinIcon;
            default: return emptySlotIcon;
        }
    }

    private Color GetColor(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield: return shieldColor;
            case PowerUpType.SpeedBoost: return speedColor;
            case PowerUpType.DoubleCoin: return coinColor;
            default: return Color.white;
        }
    }

    private string GetDisplayName(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield: return "SHIELD!";
            case PowerUpType.SpeedBoost: return "SPEED BOOST!";
            case PowerUpType.DoubleCoin: return "DOUBLE COIN!";
            default: return "";
        }
    }
}
