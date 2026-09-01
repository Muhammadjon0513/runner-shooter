using UnityEngine;
using DG.Tweening;
using TMPro;

public class Obstacle : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;
    private int coinValue; // To'siq yo'q qilinganda beriladigan coin (= maxHealth)
    
    // Visuals
    private MeshRenderer meshRenderer;
    private Color originalColor;
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;

    // BUG-3 Fix: MaterialPropertyBlock — xotira tejash
    private MaterialPropertyBlock propBlock;
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    
    // UI (Optional: Floating text for health?)
    public TextMeshProUGUI textMesh; 

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propBlock = new MaterialPropertyBlock();
        if (meshRenderer != null)
            originalColor = meshRenderer.sharedMaterial.color;
    }

    private void OnEnable()
    {
        // Random health 1 to 5 (inclusive of 1, exclusive of 6? Random.Range int is exclusive max)
        // User said 1-5.
        maxHealth = Random.Range(1, 6); 
        currentHealth = maxHealth;

        // Coin qiymati = HP × coinBonus multiplier
        float coinMultiplier = 1f;
        if (UpgradeManager.Instance != null)
            coinMultiplier = UpgradeManager.Instance.GetValue("coinBonus");
        coinValue = Mathf.RoundToInt(maxHealth * coinMultiplier);
        UpdateVisuals();
        
        // Reset color in case it was pooled while red
        if (meshRenderer != null)
        {
            propBlock.SetColor(ColorID, originalColor);
            meshRenderer.SetPropertyBlock(propBlock);
        }
            
        // Reset scale/position just in case
        transform.localScale = Vector3.one; 
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateVisuals();

        // Hit FX
        // 1. Pop up (Punch)
        transform.DOKill(true); // Kill previous tweens
        transform.DOPunchPosition(new Vector3(0, 0.5f, 0), 0.2f, 5, 1);
        
        // 2. Flash Red (BUG-3 Fix: MaterialPropertyBlock orqali)
        if (meshRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashColor());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashColor()
    {
        propBlock.SetColor(ColorID, damageColor);
        meshRenderer.SetPropertyBlock(propBlock);
        yield return new WaitForSeconds(0.05f);
        propBlock.SetColor(ColorID, originalColor);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    private void Die()
    {
        // Add coins
        if (GameManager.Instance != null)
        {
            int finalCoinValue = coinValue;

            // DoubleCoin power-up tekshirish
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null && player.HasDoubleCoin)
                finalCoinValue *= 2;

            GameManager.Instance.AddCoin(finalCoinValue);
            Debug.Log($"Coin added: +{finalCoinValue}! Total: {GameManager.Instance.TotalCoins}");
        }
        else
        {
            Debug.LogError("GameManager Instance is null!");
        }
        
        // Return to pool (disable)
        gameObject.SetActive(false);
    }
    
    // Call this if using 3D Text to show health numbers on the block
    private void UpdateVisuals()
    {
        if (textMesh != null)
        {
            textMesh.text = currentHealth.ToString();
        }
    }

    // BUG-2 Fix: Pool'ga qaytganda barcha tweenlar va coroutinelarni to'xtatish
    private void OnDisable()
    {
        transform.DOKill();
        StopAllCoroutines();
    }
}
