using UnityEngine;

public class WeaponShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float damage = 10f;          // Besar damage yang diberikan
    public float range = 100f;          // Jarak maksimal tembakan
    public float fireRate = 0.2f;       // Jeda waktu antar tembakan (untuk senjata otomatis)
    
    private float nextTimeToFire = 0f;

    [Header("Player Model Hiding")]
    public GameObject playerModel;      // Masukkan objek RobotKyle ke sini
    public GameObject weaponModel;      // Masukkan model senjata (kosongkan jika script ini ada di senjata langsung)
    private Renderer[] modelRenderers;
    private Renderer[] weaponRenderers;

    [Header("References")]
    public Camera fpsCam;               // Kamera tempat arah tembakan berasal
    public Transform muzzlePoint;       // Titik ujung senjata untuk efek api (opsional)
    public GameObject muzzleFlashPrefab;// Prefab Efek tembakan (opsional)
    public GameObject impactEffect;     // Efek peluru mengenai objek (opsional)

    [Header("UI & Damage Text")]
    public GameObject damageTextPrefab; // Prefab teks damage yang melayang
    public float headshotMultiplier = 2.5f; // Pengali damage jika mengenai kepala

    [Header("Input")]
    public KeyCode shootKey = KeyCode.Mouse0; // Klik kiri mouse

    [Header("Audio")]
    public AudioSource audioSource;     // Komponen untuk memutar suara
    public AudioClip shootSound;        // File suara tembakan

    [Header("Recoil")]
    public float recoilX = 2f;          // Hentakan vertikal (ke atas)
    public float recoilY = 0.5f;        // Hentakan horizontal (kiri/kanan)
    public float recoilZ = 0.2f;        // Hentakan rotasi miring (roll)
    public PlayerCam playerCam;         // Referensi ke script PlayerCam untuk efek recoil

    [Header("Stealth / Sound")]
    [Tooltip("Jumlah suara stealth yang dihasilkan dari 1 tembakan (0-100)")]
    public float shootNoise = 100f;

    void Awake()
    {
        // Mencari PlayerCam secara otomatis dari objek fpsCam jika belum dimasukkan
        if (playerCam == null && fpsCam != null)
        {
            playerCam = fpsCam.GetComponent<PlayerCam>();
        }

        // Mengambil semua komponen Renderer dari model pemain (RobotKyle)
        if (playerModel != null)
        {
            modelRenderers = playerModel.GetComponentsInChildren<Renderer>();
        }

        // Mengambil renderer dari senjata
        if (weaponModel == null)
        {
            weaponModel = gameObject;
        }
        Renderer[] allRenderers = weaponModel.GetComponentsInChildren<Renderer>();
        weaponRenderers = new Renderer[allRenderers.Length];
        int validCount = 0;
        foreach (Renderer r in allRenderers)
        {
            string rName = r.gameObject.name.ToLower();
            if (!rName.Contains("flash") && !rName.Contains("effect"))
            {
                weaponRenderers[validCount] = r;
                validCount++;
            }
        }
        System.Array.Resize(ref weaponRenderers, validCount);
    }

    void OnEnable()
    {
        // Reset state saat senjata diaktifkan (misal ganti senjata)
        SetModelVisibility(true);
        SetWeaponVisibility(false);
    }

    void Update()
    {
        // Jangan proses input saat game sedang dipause
        if (PauseManager.isPaused) return;

        // Cek apakah tombol tembak (Klik Kiri) sedang ditekan/ditahan
        bool isShooting = Input.GetKey(shootKey) || Input.GetKey(KeyCode.JoystickButton5);

        if (isShooting)
        {
            // Sedang menekan tombol tembak: Sembunyikan Robot, Tampilkan Senjata
            SetModelVisibility(false);
            SetWeaponVisibility(true);

            // Eksekusi tembakan jika fire rate sudah terpenuhi
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + fireRate;
                Shoot();
            }
        }
        else
        {
            // Tidak menekan tombol tembak: Tampilkan Robot, Sembunyikan Senjata
            SetModelVisibility(true);
            SetWeaponVisibility(false);
        }
    }

    void Shoot()
    {
        // Putar suara tembakan
        if (audioSource != null && shootSound != null)
        {
            // Menggunakan PlayOneShot agar suara tembakan bisa bertumpuk (tidak terpotong jika menembak cepat)
            audioSource.PlayOneShot(shootSound);
        }

        // Laporkan ke SoundMeter bahwa player sedang menembak
        if (SoundMeter.Instance != null)
        {
            SoundMeter.Instance.AddSound(shootNoise);
        }

        // Terapkan efek recoil ke kamera
        if (playerCam != null)
        {
            playerCam.ApplyRecoil(recoilX, recoilY, recoilZ);
        }

        // 1. Munculkan efek kilatan api (Muzzle Flash) di ujung senjata
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            // Munculkan prefab flash di posisi dan rotasi ujung senjata
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            // Jadikan flash sebagai child dari muzzlePoint agar ikut bergerak jika senjata digerakkan
            flash.transform.SetParent(muzzlePoint);
            Destroy(flash, 0.1f); // Hancurkan setelah 0.1 detik (kilatan api sangat cepat)
        }

        // 2. Tembakkan Raycast dari titik tengah kamera ke arah depan
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            // Debug tulisan di console (Objek apa yang tertembak)
            Debug.Log("Tembakan mengenai: " + hit.transform.name);

            // Cek apakah mengenai bagian dari bot (Hitbox Kepala atau Badan)
            BotHitbox hitbox = hit.transform.GetComponent<BotHitbox>();
            if (hitbox != null)
            {
                // Cek apakah mengenai kepala (Gunakan Tag saja agar tidak membebani memori dengan string)
                bool isHeadshot = hit.collider.CompareTag("Head");
                
                // Hitung total damage
                float finalDamage = isHeadshot ? damage * headshotMultiplier : damage;

                // Beri tahu hitbox bahwa dia tertembak dengan sejumlah damage akhir
                hitbox.OnHit(finalDamage);

                // Memunculkan Teks Damage Melayang
                if (damageTextPrefab != null)
                {
                    Vector3 textPos = hit.point + hit.normal * 0.2f + Vector3.up * 0.5f;
                    
                    GameObject dmgTextObj = null;
                    if (ObjectPoolManager.Instance != null)
                    {
                        // Ambil dari Object Pool
                        dmgTextObj = ObjectPoolManager.Instance.SpawnFromPool("DamageText", textPos, Quaternion.identity);
                    }
                    else
                    {
                        // Fallback jika lupa memasang PoolManager di Scene
                        dmgTextObj = Instantiate(damageTextPrefab, textPos, Quaternion.identity);
                    }

                    // Panggil fungsi Setup di script DamageText
                    DamageText dmgTextScript = dmgTextObj.GetComponent<DamageText>();
                    if (dmgTextScript != null)
                    {
                        dmgTextScript.Setup(finalDamage, isHeadshot);
                    }
                }
            }

            // 3. Munculkan efek peluru menabrak benda (Impact Effect)
            if (impactEffect != null)
            {
                // Instantiate efek di titik tumbukan (hit.point) dan putar efeknya sesuai permukaan benda (hit.normal)
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                
                // Hancurkan objek efek setelah 2 detik agar tidak menumpuk di memori
                Destroy(impactGO, 2f); 
            }
        }
    }

    // Fungsi untuk menyalakan/mematikan render mesh model pemain
    void SetModelVisibility(bool visible)
    {
        if (modelRenderers == null) return;
        foreach (Renderer r in modelRenderers)
        {
            r.enabled = visible;
        }
    }

    // Fungsi untuk menyalakan/mematikan render mesh senjata
    void SetWeaponVisibility(bool visible)
    {
        if (weaponRenderers == null) return;
        for (int i = 0; i < weaponRenderers.Length; i++)
        {
            weaponRenderers[i].enabled = visible;
        }
    }
}
