using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // Singleton agar mudah dipanggil dari mana saja
    public static DialogueManager Instance { get; private set; }

    [Header("UI Reference")]
    [Tooltip("Panel UI untuk menampilkan subtitle")]
    public GameObject subtitlePanel;
    [Tooltip("Text UI untuk subtitle (TextMeshProUGUI)")]
    public TextMeshProUGUI subtitleTextUI;

    [Header("Audio Reference")]
    [Tooltip("AudioSource untuk memutar suara karakter (voice over)")]
    public AudioSource dialogueAudioSource;
    
    [Tooltip("Volume suara dialog (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float dialogueVolume = 1f;

    private Coroutine hideSubtitleCoroutine;

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
        // Pastikan subtitle tersembunyi saat game dimulai
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Memutar suara dan menampilkan subtitle di layar.
    /// Otomatis menyembunyikan subtitle saat durasi suara habis.
    /// </summary>
    public void PlayDialogue(AudioClip voiceClip, string subtitleText)
    {
        // 1. Tampilkan Subtitle Text
        if (subtitlePanel != null && subtitleTextUI != null && !string.IsNullOrEmpty(subtitleText))
        {
            subtitlePanel.SetActive(true);
            subtitleTextUI.text = subtitleText;
        }

        float displayDuration = 3f; // Default durasi jika tidak ada voice clip

        // 2. Putar Voice Clip
        if (dialogueAudioSource != null && voiceClip != null)
        {
            // Hentikan suara yang sedang main jika ada
            dialogueAudioSource.Stop();
            
            dialogueAudioSource.clip = voiceClip;
            dialogueAudioSource.volume = dialogueVolume;
            dialogueAudioSource.Play();
            
            displayDuration = voiceClip.length;
        }

        // 3. Atur timer untuk menyembunyikan subtitle
        if (hideSubtitleCoroutine != null)
        {
            StopCoroutine(hideSubtitleCoroutine);
        }
        hideSubtitleCoroutine = StartCoroutine(HideSubtitleAfterDelay(displayDuration));
    }

    private IEnumerator HideSubtitleAfterDelay(float delay)
    {
        // Tunggu selama durasi yang ditentukan
        yield return new WaitForSeconds(delay);

        // Sembunyikan panel subtitle
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }
    }
}
