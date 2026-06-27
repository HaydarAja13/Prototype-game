using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    // Singleton pattern agar mudah dipanggil dari script mana saja
    public static ObjectiveManager Instance { get; private set; }

    [Header("UI Reference")]
    [Tooltip("Masukkan GameObject Panel/Background text objective di sini")]
    public GameObject objectivePanel;
    [Tooltip("Masukkan TextMeshProUGUI untuk menampilkan text objective")]
    public TextMeshProUGUI objectiveTextUI;

    [Header("Objectives List")]
    [TextArea(2, 4)]
    [Tooltip("Daftar semua objective dari awal sampai akhir level. Index 0 = Pertama, 1 = Kedua, dst.")]
    public string[] objectives;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip objectiveUpdateSound;

    [Header("State (Jangan diubah manual)")]
    public int currentObjectiveIndex = 0;

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
        // Tampilkan objective pertama saat game mulai
        UpdateObjectiveUI();
    }

    /// <summary>
    /// Menyelesaikan objective saat ini dan otomatis lanjut ke selanjutnya.
    /// Bisa dipanggil lewat Unity Event (contoh: On Access Granted di NavKeypad).
    /// </summary>
    [ContextMenu("Complete Current Objective")]
    public void CompleteCurrentObjective()
    {
        if (currentObjectiveIndex >= objectives.Length) return;

        currentObjectiveIndex++;
        
        // Mainkan efek suara
        if (audioSource != null && objectiveUpdateSound != null)
        {
            audioSource.PlayOneShot(objectiveUpdateSound);
        }

        if (currentObjectiveIndex < objectives.Length)
        {
            UpdateObjectiveUI();
            Debug.Log("[ObjectiveManager] Objective Updated: " + objectives[currentObjectiveIndex]);
        }
        else
        {
            // Level Selesai (Semua objective sudah komplit)
            if (objectiveTextUI != null)
            {
                objectiveTextUI.text = "Objective:\n- Escape / Level Completed!";
            }
            Debug.Log("[ObjectiveManager] All objectives completed!");
        }
    }

    /// <summary>
    /// Menyelesaikan objective dengan Index spesifik agar lebih aman (mencegah double-trigger).
    /// </summary>
    public void CompleteObjectiveByIndex(int targetIndex)
    {
        // Hanya selesaikan jika target index sama dengan objective saat ini
        if (currentObjectiveIndex == targetIndex)
        {
            CompleteCurrentObjective();
        }
    }

    private void UpdateObjectiveUI()
    {
        // Jika tidak ada objective sama sekali
        if (objectives == null || objectives.Length == 0)
        {
            if (objectivePanel != null) objectivePanel.SetActive(false);
            return;
        }

        // Tampilkan panel
        if (objectivePanel != null) objectivePanel.SetActive(true);

        // Update Text
        if (objectiveTextUI != null && currentObjectiveIndex < objectives.Length)
        {
            // Tambahkan animasi atau warna jika mau (contoh ini plain text)
            objectiveTextUI.text = "Objective:\n" + objectives[currentObjectiveIndex];
        }
    }
}
