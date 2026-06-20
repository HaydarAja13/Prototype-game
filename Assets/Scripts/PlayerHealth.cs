using UnityEngine;
using UnityEngine.UI; // Diperlukan untuk mengakses UI Image
using UnityEngine.SceneManagement; // Diperlukan untuk mereload Scene
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // Diubah jadi float agar pembagian fillAmount lebih akurat
    private float currentHealth;

    [Header("UI Reference")]
    public Image healthBar;            // Masukkan gambar (Image) bar HP ke sini
    
    [Header("Damage Effect (Vignette)")]
    public Image damageVignette;       // UI Image berwarna merah/vignette
    public float flashSpeed = 5f;      
    public Color flashColor = new Color(1f, 0f, 0f, 0.6f); 

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        // Pastikan vignette sepenuhnya transparan saat game mulai
        if (damageVignette != null)
        {
            damageVignette.color = Color.clear;
        }
    }

    void Update()
    {

        // ==========================================
        // EFEK LAYAR MERAH
        // ==========================================
        // Secara perlahan memudarkan layar merah kembali menjadi transparan setiap frame
        if (damageVignette != null && damageVignette.color != Color.clear)
        {
            damageVignette.color = Color.Lerp(damageVignette.color, Color.clear, flashSpeed * Time.deltaTime);
        }
    }

    // Fungsi saat terkena damage
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // Munculkan layar merah mendadak
        if (damageVignette != null)
        {
            damageVignette.color = flashColor;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateHealthBar();
    }

    // Fungsi saat menyembuhkan darah (Dari contoh internet)
    public void Heal(float healingAmount)
    {
        currentHealth += healingAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Agar nyawa tidak tembus lebih dari maxHealth
        UpdateHealthBar();
    }

    // Fungsi memperbarui UI
    void UpdateHealthBar()
    {
        // 1. UPDATE BAR GAMBAR
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth; // Logika fillAmount persis dari contoh internet
        }
    }

    // Fungsi mati
    void Die()
    {
        Debug.Log("Player Mati!");
        
        // Reload scene jika mati (Sesuai contoh internet)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
