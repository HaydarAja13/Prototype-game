using UnityEngine;

public class BotHitbox : MonoBehaviour
{
    public BotHealth mainHealth; // Referensi ke script BotHealth utama
    
    [Header("Hitbox Settings")]
    public bool isHead;          // Centang ini jika kotak tabrakan ini adalah KEPALA
    public float headshotMultiplier = 2f; // Kalikan damage 2x jika kena kepala

    // Fungsi ini akan dipanggil oleh senjata saat tertembak
    public void OnHit(float weaponDamage)
    {
        if (mainHealth != null)
        {
            float finalDamage = weaponDamage;
            
            // Jika mengenai kepala, kalikan damage
            if (isHead)
            {
                finalDamage *= headshotMultiplier;
                Debug.Log("CRITICAL HEADSHOT! Damage: " + finalDamage);
            }

            // Teruskan damage ke nyawa utama bot
            mainHealth.TakeDamage(finalDamage);
        }
    }
}
