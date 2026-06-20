using UnityEngine;
using UnityEngine.UI;

public class SoundMeter : MonoBehaviour
{
    public static SoundMeter Instance { get; private set; }

    [Header("Sound Settings")]
    [Tooltip("Batas maksimal suara sebelum musuh menyadari posisi player")]
    public float maxSound = 100f;
    [Tooltip("Kecepatan suara menghilang (decay) per detik saat player diam")]
    public float soundDecayRate = 20f;
    
    [Header("UI Reference")]
    [Tooltip("Drag UI Image (tipe Filled) ke sini untuk menampilkan meteran suara")]
    public Image soundMeterFill;

    [Header("State")]
    public float currentSound = 0f;
    private bool isDetected = false;
    
    [Tooltip("Jeda waktu setelah ketahuan sebelum bar suara bisa mulai turun lagi")]
    public float detectCooldown = 3f;
    private float detectCooldownTimer = 0f;

    // Event yang akan dipanggil saat suara penuh
    public static event System.Action<Transform> OnMaxSoundReached;

    // Referensi ke posisi player
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Cari player di scene
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            // Fallback mencari objek yang punya PlayerMov
            PlayerMov pm = FindAnyObjectByType<PlayerMov>();
            if (pm != null) playerTransform = pm.transform;
        }
    }

    private void Update()
    {
        // Proses pengurangan suara seiring waktu
        if (isDetected)
        {
            // Jika sedang ketahuan, bar ditahan di penuh selama cooldown
            detectCooldownTimer -= Time.deltaTime;
            if (detectCooldownTimer <= 0f)
            {
                isDetected = false;
            }
        }
        else
        {
            if (currentSound > 0f)
            {
                currentSound -= soundDecayRate * Time.deltaTime;
                currentSound = Mathf.Clamp(currentSound, 0f, maxSound);
            }
        }

        // Update UI Image
        if (soundMeterFill != null)
        {
            soundMeterFill.fillAmount = currentSound / maxSound;
        }
    }

    /// <summary>
    /// Dipanggil dari PlayerMov atau WeaponShooting untuk menambah nilai suara
    /// </summary>
    public void AddSound(float amount)
    {
        if (isDetected) return; // Jangan tambah suara kalau sudah ketahuan penuh

        currentSound += amount;
        currentSound = Mathf.Clamp(currentSound, 0f, maxSound);

        if (currentSound >= maxSound)
        {
            TriggerDetection();
        }
    }

    private void TriggerDetection()
    {
        isDetected = true;
        detectCooldownTimer = detectCooldown;
        
        Debug.Log("[Sound Meter] Suara Maksimal! Player Terdeteksi!");
        
        // Panggil event untuk semua musuh yang mendaftar
        if (OnMaxSoundReached != null && playerTransform != null)
        {
            OnMaxSoundReached.Invoke(playerTransform);
        }
    }
}
