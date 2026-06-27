using UnityEngine;
using UnityEngine.SceneManagement;

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

    void Start()
    {
        // Pastikan UI pause tidak aktif saat game baru dimulai
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Pastikan waktu normal dan audio menyala saat awal
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
    }

    void Update()
    {
        // Tekan tombol Escape (ESC) atau Start (JoystickButton7) untuk memunculkan atau menyembunyikan Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Fungsi untuk menjeda (pause) game.
    /// </summary>
    public void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true); // Tampilkan UI Pause
        }

        Time.timeScale = 0f; // Hentikan waktu game
        isPaused = true;
        
        // Hentikan sementara semua suara (opsional, bisa dihapus jika ingin BGM tetap bunyi)
        AudioListener.pause = true; 

        // Buka kunci dan tampilkan kursor mouse agar pemain bisa klik tombol UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Fungsi untuk melanjutkan game (Resume).
    /// Pasangkan fungsi ini ke event OnClick() pada tombol "Resume".
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false); // Sembunyikan UI Pause
        }

        Time.timeScale = 1f; // Kembalikan waktu berjalan normal
        isPaused = false;
        
        // Kembalikan suara game
        AudioListener.pause = false;

        // Kunci kembali kursor dan sembunyikan agar cocok untuk game FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Fungsi untuk keluar ke Main Menu.
    /// Pasangkan fungsi ini ke event OnClick() pada tombol "Main Menu".
    /// </summary>
    public void GoToMainMenu()
    {
        // PENTING: Kembalikan waktu berjalan normal dan suara nyala sebelum pindah scene.
        // Jika tidak, saat main lagi gamenya akan macet!
        Time.timeScale = 1f; 
        isPaused = false;
        AudioListener.pause = false;

        Debug.Log("Loading Main Menu: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
