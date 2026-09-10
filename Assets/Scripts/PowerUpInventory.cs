using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Blur uslubidagi 3-slot power-up inventar tizimi.
/// Power-up yig'ilganda inventarga tushadi, o'yinchi istagan vaqtda ishlatadi.
/// </summary>
public class PowerUpInventory : MonoBehaviour
{
    public static PowerUpInventory Instance;

    [Header("Settings")]
    public int maxSlots = 3;

    // Inventardagi power-up'lar ro'yxati
    private List<PowerUpType> inventory = new List<PowerUpType>();

    // Event'lar — UI yangilash uchun
    public event Action<List<PowerUpType>> OnInventoryChanged;
    public event Action<PowerUpType> OnPowerUpUsed;

    public int Count => inventory.Count;
    public bool IsFull => inventory.Count >= maxSlots;

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

    /// <summary>
    /// Power-up yig'ilganda inventarga qo'shish.
    /// Agar inventar to'la bo'lsa — false qaytaradi (yig'ilmaydi).
    /// </summary>
    public bool AddPowerUp(PowerUpType type)
    {
        if (inventory.Count >= maxSlots)
        {
            Debug.Log($"Inventar to'la ({maxSlots}/{maxSlots})! Power-up yig'ilmadi.");
            return false;
        }

        inventory.Add(type);
        OnInventoryChanged?.Invoke(inventory);
        Debug.Log($"Power-up yig'ildi: {type} ({inventory.Count}/{maxSlots})");
        return true;
    }

    /// <summary>
    /// Birinchi (aktiv) slotdagi power-up'ni ishlatish.
    /// </summary>
    public void UseFirstSlot()
    {
        if (inventory.Count == 0)
        {
            Debug.Log("Inventar bo'sh!");
            return;
        }

        PowerUpType type = inventory[0];
        inventory.RemoveAt(0);

        // PlayerController'ga yuborish
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ActivatePowerUp(type);
            OnPowerUpUsed?.Invoke(type);
        }

        OnInventoryChanged?.Invoke(inventory);
        Debug.Log($"Power-up ishlatildi: {type} ({inventory.Count}/{maxSlots})");
    }

    /// <summary>
    /// Ma'lum indexdagi slotni ishlatish.
    /// </summary>
    public void UseSlot(int index)
    {
        if (index < 0 || index >= inventory.Count) return;

        PowerUpType type = inventory[index];
        inventory.RemoveAt(index);

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ActivatePowerUp(type);
            OnPowerUpUsed?.Invoke(type);
        }

        OnInventoryChanged?.Invoke(inventory);
    }

    /// <summary>
    /// Hozirgi inventar holatini olish (UI uchun).
    /// </summary>
    public List<PowerUpType> GetInventory()
    {
        return new List<PowerUpType>(inventory);
    }

    /// <summary>
    /// Inventarni tozalash (GameOver'da).
    /// </summary>
    public void ClearInventory()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke(inventory);
    }
}
