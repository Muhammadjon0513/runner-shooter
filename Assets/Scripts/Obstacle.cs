using UnityEngine;
using DG.Tweening;
using TMPro;

public class Obstacle : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;
    
    // Visuals
    private MeshRenderer meshRenderer;
    private Color originalColor;
    public Color damageColor = Color.red;
    public float flashDuration = 0.1f;
    
    // UI (Optional: Floating text for health?)
    public TextMeshProUGUI textMesh; 

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
    }

    private void OnEnable()
    {
        // Random health 1 to 5 (inclusive of 1, exclusive of 6? Random.Range int is exclusive max)
        // User said 1-5.
        maxHealth = Random.Range(1, 6); 
        currentHealth = maxHealth;
        UpdateVisuals();
        
        // Reset color in case it was pooled while red
        if (meshRenderer != null)
            meshRenderer.material.color = originalColor;
            
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
        
        // 2. Flash Red
        if (meshRenderer != null)
        {
            meshRenderer.material.DOColor(damageColor, 0.05f).OnComplete(() =>
            {
                meshRenderer.material.DOColor(originalColor, 0.05f);
            });
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Add coins
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin(1); // 1 coin per obstacle
            Debug.Log($"Coin added! Total: {GameManager.Instance.Coins}");
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
}
