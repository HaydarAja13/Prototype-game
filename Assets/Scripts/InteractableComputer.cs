using UnityEngine;

public class InteractableComputer : MonoBehaviour
{
    [Header("Computer Settings")]
    public string computerName = "Computer";
    
    [Tooltip("Video Player object to activate/play when floppy disk is used")]
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public GameObject videoDisplayObject;

    public void PlayVideo()
    {
        if (videoDisplayObject != null)
        {
            videoDisplayObject.SetActive(true);
        }
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }
}
