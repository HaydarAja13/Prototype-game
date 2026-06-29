using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk fungsi pindah scene
using UnityEngine.EventSystems; // Ditambahkan untuk sistem navigasi UI

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Ketik persis nama file Scene utama gamemu di sini")]
    public string namaSceneGame = "GameScene"; // <-- Ganti "GameScene" dengan nama file scenemu

    [Header("UI Navigation")]
    [Tooltip("Tombol pertama yang akan otomatis terpilih di Main Menu (misal: Tombol Play)")]
    public GameObject firstSelectedButton;

    private void Start()
    {
        // Otomatis memilih tombol pertama saat main menu dimulai (seperti di konsol)
        if (firstSelectedButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    private void Update()
    {
        // Jika karena suatu alasan tidak ada tombol yang terpilih (misal karena klik sembarangan)
        // dan pemain menggerakkan D-pad atau Analog, otomatis pilih tombol pertama lagi
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
            {
                if (firstSelectedButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                }
            }
        }
    }

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
