using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Masukkan Panel UI Pause Menu (GameObject) ke sini")]
    public GameObject pauseMenuUI;

    [Header("Scene Settings")]
    [Tooltip("Ketik nama scene Main Menu kamu di sini")]
    public string mainMenuSceneName = "MainMenu";

    // Status statis untuk mengecek apakah game sedang dipause dari script lain
    public static bool isPaused = false;

    // =====================================================================
    // TimeScale sangat kecil agar EventSystem/InputModule tetap hidup
    // Semua gameplay sudah diblokir oleh guard 'isPaused' di setiap script.
    // =====================================================================
    private const float PAUSED_TIMESCALE = 0.0001f;

    void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
            SetupPauseCanvas();
        }
        
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
    }

    /// <summary>
    /// Setup Canvas khusus pada pause panel agar:
    /// 1. Render di ATAS semua UI game lain (vignette, crosshair, dsb)
    /// 2. Punya GraphicRaycaster sendiri agar deteksi klik independen
    /// 3. CanvasGroup mengizinkan interaksi
    /// Tanpa ini, elemen UI transparan seperti damageVignette (Image fullscreen
    /// dengan raycastTarget=true) bisa memblokir klik ke tombol pause.
    /// </summary>
    private void SetupPauseCanvas()
    {
        // Tambah Canvas override agar sorting order independen
        Canvas pauseCanvas = pauseMenuUI.GetComponent<Canvas>();
        if (pauseCanvas == null)
        {
            pauseCanvas = pauseMenuUI.AddComponent<Canvas>();
        }
        pauseCanvas.overrideSorting = true;
        pauseCanvas.sortingOrder = 999; // Pastikan paling atas

        // Tambah GraphicRaycaster khusus agar pause panel mendeteksi klik sendiri
        if (pauseMenuUI.GetComponent<GraphicRaycaster>() == null)
        {
            pauseMenuUI.AddComponent<GraphicRaycaster>();
        }

        // Jika ada CanvasGroup, pastikan interaksi diizinkan
        CanvasGroup cg = pauseMenuUI.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }

    void Update()
    {
        // Tekan tombol Escape (ESC) atau tombol Start/Options di controller untuk memunculkan atau menyembunyikan Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                // Cek apakah player sedang inspect atau menggunakan CCTV. 
                // Jika ya, biarkan script bersangkutan yang memproses tombol Escape.
                bool isInspecting = (PlayerInventory.Instance != null && PlayerInventory.Instance.isInspecting);
                bool isViewingCCTV = (CCTVManager.Instance != null && CCTVManager.Instance.isViewingCCTV);

                if (!isInspecting && !isViewingCCTV)
                {
                    PauseGame();
                }
            }
        }

        // Jika sedang pause, tekan Q atau tombol Select/Share di controller untuk kembali ke Main Menu
        if (isPaused && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton6)))
        {
            GoToMainMenu();
        }

        // Safety: pastikan cursor tetap terbuka selama pause
        if (isPaused && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Fungsi untuk menjeda (pause) game.
    /// </summary>
    public void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        Time.timeScale = PAUSED_TIMESCALE;
        isPaused = true;
        AudioListener.pause = true; 

        // Buka kunci dan tampilkan kursor mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Reset input module agar pointer tracking dimulai ulang
        // setelah cursor berubah dari Locked ke None
        StartCoroutine(ForceResetInputModule());
    }

    /// <summary>
    /// Paksa reset InputModule pada EventSystem.
    /// Ketika cursor berubah dari Locked ke None, InputSystemUIInputModule
    /// kadang tidak langsung mendeteksi posisi pointer baru.
    /// Dengan me-restart module, tracking dimulai ulang dari posisi cursor saat ini.
    /// </summary>
    private IEnumerator ForceResetInputModule()
    {
        // Tunggu 1 frame agar cursor benar-benar ter-unlock oleh OS
        yield return null;

        if (EventSystem.current != null)
        {
            BaseInputModule module = EventSystem.current.currentInputModule;
            if (module != null)
            {
                // Matikan lalu nyalakan ulang untuk force re-init pointer state
                module.enabled = false;
                // Tunggu 1 frame lagi
                yield return null;
                module.enabled = true;
                Debug.Log("[PauseManager] Input module reset berhasil.");
            }
        }
    }

    /// <summary>
    /// Fungsi untuk melanjutkan game (Resume).
    /// Pasangkan fungsi ini ke event OnClick() pada tombol "Resume".
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;

        // Kunci kembali kursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Fungsi untuk keluar ke Main Menu.
    /// Pasangkan fungsi ini ke event OnClick() pada tombol "Main Menu".
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        isPaused = false;
        AudioListener.pause = false;

        Debug.Log("Loading Main Menu: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
