using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    [Header("Pengaturan Pintu")]
    [Tooltip("Seberapa jauh pintu akan turun ke bawah? (misal: 4 meter)")]
    public float jarakTurun = 4f;

    [Tooltip("Kecepatan pintu bergerak turun")]
    public float kecepatan = 2f;

    private Vector3 posisiAwal;
    private Vector3 posisiTerbuka;
    private bool sedangTerbuka = false;

    void Start()
    {
        // Simpan posisi awal pintu saat game baru mulai
        posisiAwal = transform.position;
        
        // Hitung posisi akhir pintu (turun ke bawah sumbu Y)
        posisiTerbuka = posisiAwal - new Vector3(0, jarakTurun, 0);
    }

    void Update()
    {
        // Jika pintu diperintahkan untuk terbuka, gerakkan pelan-pelan ke bawah
        if (sedangTerbuka)
        {
            transform.position = Vector3.MoveTowards(transform.position, posisiTerbuka, kecepatan * Time.deltaTime);
        }
    }

    // Fungsi ini akan dipanggil oleh Keypad saat "Access Granted"
    public void BukaPintu()
    {
        sedangTerbuka = true;
    }
}
