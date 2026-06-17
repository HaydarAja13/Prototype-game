using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10; // Jumlah damage yang diberikan ke player
    
    [Header("Lifetime Settings")]
    public float lifetime = 5f; // Waktu maksimal peluru ada di scene sebelum hancur otomatis

    void Awake()
    {
        // Menambahkan efek jejak (Bullet Trail) secara otomatis agar peluru sangat mudah dilihat
        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr == null)
        {
            tr = gameObject.AddComponent<TrailRenderer>();
            tr.time = 0.2f; // Seberapa lama jejak bertahan (panjang jejak)
            tr.startWidth = 0.2f; // Lebar awal jejak
            tr.endWidth = 0f;     // Lebar akhir jejak (mengecil)
            
            // Gunakan material dasar yang tidak membutuhkan cahaya (unlit) agar selalu terang
            tr.material = new Material(Shader.Find("Sprites/Default"));
            
            // Warna peluru (Kuning terang menyala memudar menjadi merah/transparan di ekornya)
            tr.startColor = new Color(1f, 0.8f, 0f, 1f); 
            tr.endColor = new Color(1f, 0.1f, 0f, 0f);
            
            tr.minVertexDistance = 0.05f; // Membuat jejak lebih mulus
        }
    }

    void Start()
    {
        // Hancurkan proyektil setelah beberapa waktu agar tidak memenuhi memori jika meleset
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Periksa apakah objek yang ditabrak memiliki komponen PlayerHealth
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            // Jika yang ditabrak adalah Player, berikan damage
            playerHealth.TakeDamage(damage);
        }

        // Hancurkan peluru setelah menabrak sesuatu (entah itu player, tanah, atau tembok)
        Destroy(gameObject);
    }
}
