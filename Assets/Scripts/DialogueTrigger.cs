using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("Suara dialog yang akan diputar")]
    public AudioClip voiceClip;
    
    [Tooltip("Teks subtitle dialog")]
    [TextArea(3, 5)]
    public string subtitleText;

    /// <summary>
    /// Panggil fungsi ini dari Unity Event (misal: On Access Granted di Keypad, OnClick di Button, dll)
    /// </summary>
    public void TriggerDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.PlayDialogue(voiceClip, subtitleText);
        }
        else
        {
            Debug.LogWarning("DialogueManager tidak ditemukan di scene!");
        }
    }
}
