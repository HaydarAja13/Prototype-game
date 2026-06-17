using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk fungsi pindah scene

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Ketik persis nama file Scene utama gamemu di sini")]
    public string namaSceneGame = "GameScene"; // <-- Ganti "GameScene" dengan nama file scenemu

    /// <summary>
    /// Fungsi ini akan dipanggil saat tombol Play ditekan
    /// </summary>
    public void PlayGame()
    {
        Debug.Log("Memuat scene: " + namaSceneGame);
        
        // Memuat scene berdasarkan nama
        SceneManager.LoadScene(namaSceneGame);
    }

    /// <summary>
    /// Fungsi ini akan dipanggil saat tombol Exit/Quit ditekan
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Game ditutup!");
        
        // Perintah untuk menutup aplikasi (Hanya berfungsi saat game sudah di-build menjadi .exe/.apk)
        Application.Quit();

        // Baris khusus agar saat menekan tombol exit di dalam Unity Editor, Play Mode ikut berhenti
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
