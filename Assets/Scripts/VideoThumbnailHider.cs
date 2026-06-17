using UnityEngine;
using UnityEngine.Video; // Penting untuk mengakses sistem video

public class VideoThumbnailHider : MonoBehaviour
{
    [Header("Masukkan Video Player dari Hierarchy ke sini")]
    public VideoPlayer videoTarget;

    void Update()
    {
        // Mengecek terus-menerus, jika video sudah mulai berputar...
        if (videoTarget != null && videoTarget.isPlaying)
        {
            // Matikan gambar thumbnail ini agar videonya terlihat
            gameObject.SetActive(false); 
        }
    }
}
