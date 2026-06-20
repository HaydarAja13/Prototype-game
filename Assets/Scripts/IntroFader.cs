using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class IntroFader : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Lama waktu (dalam detik) sebelum panel mulai menghilang")]
    public float delayBeforeFade = 1.0f;
    
    [Tooltip("Kecepatan transisi fade out (semakin kecil semakin lambat)")]
    public float fadeSpeed = 1.5f;

    private CanvasGroup canvasGroup;
    private bool startFading = false;
    private float timer = 0f;

    void Start()
    {
        // Ambil komponen Canvas Group
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Pastikan panel terlihat sepenuhnya di awal
        canvasGroup.alpha = 1f;
        
        // Pastikan objeknya aktif
        gameObject.SetActive(true);
    }

    void Update()
    {
        // Tunggu delay sebelum mulai fade out
        if (!startFading)
        {
            timer += Time.deltaTime;
            if (timer >= delayBeforeFade)
            {
                startFading = true;
            }
            return;
        }

        // Proses Fade Out
        if (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
        }
        else
        {
            // Jika sudah sepenuhnya transparan, matikan (disable) objeknya agar tidak membebani performa
            gameObject.SetActive(false);
        }
    }
}
