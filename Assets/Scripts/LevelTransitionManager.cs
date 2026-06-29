using UnityEngine;
using System.Collections;

public class LevelTransitionManager : MonoBehaviour
{
    [Header("Pengaturan Pintu")]
    [Tooltip("Objek pintu yang akan bergerak")]
    public Transform pintuTransform;
    [Tooltip("Jarak pergerakan pintu saat terbuka (positif = turun, negatif = naik). Misal: 4")]
    public float jarakBuka = 4f;
    [Tooltip("Kecepatan pergerakan pintu")]
    public float kecepatan = 2f;
    
    [Header("Mekanisme Penutupan (Point of No Return)")]
    [Tooltip("Apakah pintu akan tertutup kembali secara otomatis?")]
    public bool tutupOtomatis = true;
    [Tooltip("Berapa detik pintu terbuka sebelum tertutup kembali secara otomatis?")]
    public float waktuTungguTutup = 5f;

    [Header("Manajemen Memori (Optimisasi)")]
    [Tooltip("Level 3 / ruangan selanjutnya yang akan diaktifkan saat access granted")]
    public GameObject levelSelanjutnya;
    [Tooltip("Daftar objek (Level 1, Level 2, dll) yang akan dinonaktifkan SETELAH pintu tertutup")]
    public GameObject[] levelUntukDimatikan;

    [Header("Audio Efek")]
    [Tooltip("Suara yang diputar saat level sebelumnya hancur/dimatikan")]
    public AudioClip destructionSound;
    [Tooltip("Volume suara kehancuran (0.0 sampai 1.0)")]
    [Range(0f, 1f)]
    public float destructionVolume = 1f;

    private Vector3 posisiTertutup;
    private Vector3 posisiTerbuka;
    private bool sedangBergerak = false;
    private Vector3 targetPosisi;

    void Start()
    {
        if (pintuTransform != null)
        {
            // Simpan posisi awal sebagai posisi tertutup
            posisiTertutup = pintuTransform.position;
            // Hitung posisi terbuka (bergerak ke bawah sumbu Y sebesar jarakBuka)
            posisiTerbuka = posisiTertutup - new Vector3(0, jarakBuka, 0);
        }
    }

    void Update()
    {
        // Menggerakkan pintu secara mulus jika sedangBergerak bernilai true
        if (sedangBergerak && pintuTransform != null)
        {
            pintuTransform.position = Vector3.MoveTowards(pintuTransform.position, targetPosisi, kecepatan * Time.deltaTime);

            // Hentikan pergerakan jika sudah sangat dekat dengan target
            if (Vector3.Distance(pintuTransform.position, targetPosisi) < 0.01f)
            {
                sedangBergerak = false;
            }
        }
    }

    // Fungsi ini dipanggil dari Event "On Access Granted" pada NavKeypad
    public void MulaiTransisi()
    {
        // 1. Nyalakan Level Selanjutnya (Level 3) terlebih dahulu
        if (levelSelanjutnya != null)
        {
            levelSelanjutnya.SetActive(true);
        }

        // 2. Mulai Buka Pintu
        if (pintuTransform != null)
        {
            targetPosisi = posisiTerbuka;
            sedangBergerak = true;

            // Jika disetting untuk tutup otomatis setelah beberapa saat
            if (tutupOtomatis)
            {
                StartCoroutine(TutupPintuDanMatikanLevel());
            }
            else
            {
                // Jika tidak otomatis tutup, langsung matikan level lama
                MatikanLevelLama();
            }
        }
        else
        {
            // Jika tidak ada referensi pintu, langsung matikan level lama
            MatikanLevelLama();
        }
    }

    // Coroutine untuk memberi jeda waktu sebelum pintu ditutup lagi
    private IEnumerator TutupPintuDanMatikanLevel()
    {
        // Tunggu selama waktu yang ditentukan agar player bisa lewat
        yield return new WaitForSeconds(waktuTungguTutup);

        // Perintahkan pintu untuk kembali ke posisi awal (tertutup)
        targetPosisi = posisiTertutup;
        sedangBergerak = true;

        // Beri waktu sebentar sampai pintu benar-benar tertutup sebelum mematikan level sebelumnya
        // Rumus sederhana: Waktu = Jarak / Kecepatan
        float estimasiWaktuGerak = jarakBuka / kecepatan;
        yield return new WaitForSeconds(estimasiWaktuGerak + 0.5f);

        // Setelah pintu tertutup, matikan level 1 dan 2 agar player fokus di level 3
        MatikanLevelLama();
    }

    // Fungsi untuk mendisable (SetActive false) objek-objek level sebelumnya
    private void MatikanLevelLama()
    {
        // Putar efek suara kehancuran jika ada
        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position, destructionVolume);
        }

        foreach (GameObject level in levelUntukDimatikan)
        {
            if (level != null)
            {
                level.SetActive(false);
            }
        }
    }
}
