using System.Collections.Generic;
using UnityEngine;

public class CCTVManager : MonoBehaviour
{
    public static CCTVManager Instance { get; private set; }

    [Header("CCTV Settings")]
    [Tooltip("Masukkan semua kamera CCTV yang ada di scene ke dalam list ini")]
    public List<Camera> cctvCameras = new List<Camera>();
    private int currentCameraIndex = 0;
    
    [Header("Audio Settings")]
    [Tooltip("AudioSource untuk memutar suara CCTV (bisa ditambahkan komponen AudioSource di GameObject ini)")]
    public AudioSource audioSource;
    [Tooltip("Suara saat pertama kali masuk mode CCTV")]
    public AudioClip enterCCTVSound;
    [Tooltip("Suara saat berganti kamera CCTV")]
    public AudioClip switchCCTVSound;

    [Header("Player References")]
    [Tooltip("Drag Canvas yang berisi Crosshair, Health, dan UI lainnya agar disembunyikan saat masuk CCTV")]
    public GameObject playerUI; 
    
    [Tooltip("Drag GameObject Player utama yang memiliki script PlayerMov")]
    public GameObject playerObj;
    
    [Tooltip("Drag Kamera FPS Player")]
    public Camera playerCamera;

    private bool isViewingCCTV = false;
    private float switchCooldown = 0f;
    
    // Referensi script untuk dimatikan sementara
    private MonoBehaviour[] playerScriptsToDisable;

    private void Awake()
    {
        // Singleton pattern
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
        // Matikan semua kamera CCTV di awal permainan
        foreach (var cam in cctvCameras)
        {
            if (cam != null)
                cam.gameObject.SetActive(false);
        }

        // Kumpulkan referensi script movement dan combat jika playerObj tidak null
        if (playerObj != null)
        {
            // Ambil semua script yang ada di player (seperti PlayerMov, PlayerCam, WeaponShooting, dll)
            // agar player tidak bisa bergerak/menembak saat sedang melihat CCTV.
            // Kita tidak mengambil seluruhnya agar script seperti PlayerHealth atau audio mungkin tetap jalan.
            // Alternatif termudah: Disable MonoBehaviour spesifik
            playerScriptsToDisable = playerObj.GetComponentsInChildren<MonoBehaviour>();
        }
    }

    private void Update()
    {
        if (isViewingCCTV)
        {
            // Kurangi cooldown timer
            if (switchCooldown > 0)
            {
                switchCooldown -= Time.deltaTime;
            }

            // Pindah ke kamera berikutnya dengan menekan E (jika cooldown sudah habis)
            if (Input.GetKeyDown(KeyCode.E) && switchCooldown <= 0f)
            {
                Debug.Log($"[CCTV Manager] Berpindah kamera. Total CCTV: {cctvCameras.Count}");
                SwitchToNextCamera();
            }

            // Keluar dari mode CCTV dengan menekan Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[CCTV Manager] Keluar dari CCTV");
                ExitCCTVMode();
            }
        }
    }

    public void EnterCCTVMode(int startingIndex = 0)
    {
        if (isViewingCCTV) return; // Cegah re-enter jika sudah di dalam CCTV

        if (cctvCameras == null || cctvCameras.Count == 0)
        {
            Debug.LogWarning("Tidak ada kamera CCTV yang didaftarkan di CCTVManager!");
            return;
        }

        isViewingCCTV = true;
        switchCooldown = 0.5f; // Beri jeda 0.5 detik sebelum bisa ganti kamera
        
        // Atur index sesuai terminal yang dihack
        if (startingIndex >= 0 && startingIndex < cctvCameras.Count)
        {
            currentCameraIndex = startingIndex;
        }
        else
        {
            currentCameraIndex = 0;
        }

        // 1. Sembunyikan UI Player (Crosshair, Hint, HP, dll)
        if (playerUI != null) playerUI.SetActive(false);

        // 2. Matikan kontrol Player (Movement, Camera Look, Shooting)
        DisablePlayerControls();

        // 3. Matikan kamera player
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);

        // 4. Nyalakan kamera CCTV pertama
        EnableCurrentCCTV();

        // 5. Mainkan suara masuk CCTV
        if (audioSource != null && enterCCTVSound != null)
        {
            audioSource.PlayOneShot(enterCCTVSound);
        }
    }

    public void ExitCCTVMode()
    {
        isViewingCCTV = false;

        // 1. Matikan semua kamera CCTV
        foreach (var cam in cctvCameras)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(false);
            }
        }

        // 2. Nyalakan kembali UI Player
        if (playerUI != null) playerUI.SetActive(true);

        // 3. Nyalakan kembali kontrol Player
        EnablePlayerControls();

        // 4. Nyalakan kembali kamera player
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
    }

    private void SwitchToNextCamera()
    {
        // Beri jeda lagi agar tidak tertekan 2 kali dengan cepat
        switchCooldown = 0.2f;

        // Matikan kamera saat ini
        if (cctvCameras[currentCameraIndex] != null)
        {
            cctvCameras[currentCameraIndex].gameObject.SetActive(false);
        }

        // Geser index (kembali ke 0 jika sudah mencapai maksimal)
        currentCameraIndex = (currentCameraIndex + 1) % cctvCameras.Count;

        // Nyalakan kamera selanjutnya
        EnableCurrentCCTV();
        
        Debug.Log($"[CCTV Manager] Sekarang melihat kamera index: {currentCameraIndex}");

        // Mainkan suara ganti kamera
        if (audioSource != null && switchCCTVSound != null)
        {
            audioSource.PlayOneShot(switchCCTVSound);
        }
    }

    private void EnableCurrentCCTV()
    {
        if (cctvCameras[currentCameraIndex] != null)
        {
            Camera cam = cctvCameras[currentCameraIndex];
            cam.gameObject.SetActive(true);
            
            // Tambahkan dan aktifkan AudioListener agar kita bisa mendengar suara dari sudut pandang CCTV ini
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener == null)
            {
                listener = cam.gameObject.AddComponent<AudioListener>();
            }
            listener.enabled = true;
        }
    }

    private void DisablePlayerControls()
    {
        if (playerObj == null) return;

        // Cari script PlayerInteract dan matikan agar tidak memencet E dua kali
        var interact = playerObj.GetComponentInChildren<PlayerInteract>();
        if (interact == null && playerCamera != null) interact = playerCamera.GetComponent<PlayerInteract>();
        if (interact != null) interact.enabled = false;

        // Cari manual komponen yang perlu dimatikan untuk menghindari mematikan script penting
        var mov = playerObj.GetComponentInChildren<PlayerMov>();
        if (mov != null) mov.enabled = false;

        // Biasanya PlayerCam ada di objek kamera
        if (playerCamera != null)
        {
            var camScript = playerCamera.GetComponent<MonoBehaviour>(); // PlayerCam is likely a MonoBehaviour on the camera
            var allCamScripts = playerCamera.GetComponents<MonoBehaviour>();
            foreach(var s in allCamScripts) 
            {
                // Disable PlayerCam to prevent mouse look
                if(s.GetType().Name == "PlayerCam" || s.GetType().Name == "WeaponShooting" || s.GetType().Name == "PlayerInteract")
                {
                    s.enabled = false;
                }
            }
        }

        // Cari script combat seperti WeaponSwitcher dan WeaponShooting (bisa ada di player atau children)
        var allScripts = playerObj.GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in allScripts)
        {
            string scriptName = s.GetType().Name;
            if (scriptName == "WeaponShooting" || scriptName == "WeaponSwitcher" || scriptName == "PlayerCam")
            {
                s.enabled = false;
            }
        }
        
        // Disable rigidbody physics temporary (stop sliding/moving)
        var rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void EnablePlayerControls()
    {
        if (playerObj == null) return;

        // Nyalakan kembali PlayerInteract
        var interact = playerObj.GetComponentInChildren<PlayerInteract>();
        if (interact == null && playerCamera != null) interact = playerCamera.GetComponent<PlayerInteract>();
        if (interact != null) interact.enabled = true;

        var mov = playerObj.GetComponentInChildren<PlayerMov>();
        if (mov != null) mov.enabled = true;

        if (playerCamera != null)
        {
            var allCamScripts = playerCamera.GetComponents<MonoBehaviour>();
            foreach(var s in allCamScripts) 
            {
                if(s.GetType().Name == "PlayerCam" || s.GetType().Name == "WeaponShooting" || s.GetType().Name == "PlayerInteract")
                {
                    s.enabled = true;
                }
            }
        }

        var allScripts = playerObj.GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in allScripts)
        {
            string scriptName = s.GetType().Name;
            if (scriptName == "WeaponShooting" || scriptName == "WeaponSwitcher" || scriptName == "PlayerCam")
            {
                s.enabled = true;
            }
        }
        
        // Re-enable rigidbody physics
        var rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}
