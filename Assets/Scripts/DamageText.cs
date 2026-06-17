using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float moveSpeed = 2f;
    public float destroyTime = 1f;

    [Header("Colors")]
    public Color normalDamageColor = Color.white;
    public Color headshotDamageColor = Color.red;

    private Color textColor;
    private float fadeTimer;

    private float initialFontSize;

    void Awake()
    {
        // Mencari komponen TextMeshPro jika belum dimasukkan
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
        
        // Simpan ukuran font asli agar tidak terus membesar saat di-reuse dari pool
        if (textMesh != null)
        {
            initialFontSize = textMesh.fontSize;
        }
    }

    public void Setup(float damageAmount, bool isHeadshot)
    {
        // Batalkan perintah mematikan (jika objek dipanggil lagi dari pool sebelum waktunya habis)
        CancelInvoke("DeactivateText");

        textMesh.text = "-" + damageAmount.ToString("F0"); // Contoh: "-10"

        // Kembalikan ke ukuran font semula sebelum diperbesar
        textMesh.fontSize = initialFontSize;

        if (isHeadshot)
        {
            textMesh.color = headshotDamageColor;
            // Membuat font lebih besar jika headshot
            textMesh.fontSize = initialFontSize * 1.5f; 
        }
        else
        {
            textMesh.color = normalDamageColor;
        }

        textColor = textMesh.color;
        fadeTimer = destroyTime;

        // Matikan objek (kembalikan ke pool) setelah destroyTime
        Invoke("DeactivateText", destroyTime);
    }

    void DeactivateText()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        // 1. Memindahkan teks ke atas secara perlahan (Floating)
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Membuat teks selalu menghadap ke arah kamera utama (Billboard effect)
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        // 3. Membuat teks perlahan menghilang (Fade out alpha)
        fadeTimer -= Time.deltaTime;
        float alpha = fadeTimer / destroyTime;
        
        textColor.a = alpha;
        textMesh.color = textColor;
    }
}
