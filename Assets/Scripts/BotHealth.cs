using UnityEngine;
using System.Collections;

public class BotHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    

    void Start()
    {
        // Set nyawa awal menjadi penuh
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        // Jika sudah mati, abaikan damage baru
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " menerima damage " + amount + ". Sisa nyawa: " + currentHealth);

        // Cek jika nyawa habis
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " Mati!");
        Destroy(gameObject);
    }
}
