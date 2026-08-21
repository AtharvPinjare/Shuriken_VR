using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System;

public class GestureTutorialStep : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text instructionText;

    [Header("Gesture Video")]
    [Tooltip("Raw Image that displays the gesture demonstration video.")]
    public GameObject gestureVideo;

    [Header("Content")]
    [TextArea]
    public string instructionMessage =
        "Perform the gesture shown in the video.";

    [Header("Video Settings")]
    public bool loopVideo = true;

    public event Action OnGestureCompleted;

    private bool isActive = false;

    public void BeginStep()
    {
        isActive = true;

        gameObject.SetActive(true);

        if (instructionText != null)
            instructionText.text = instructionMessage;

        // Show video
        if (gestureVideo != null)
            gestureVideo.SetActive(true);

        // Start video
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = loopVideo;

            videoPlayer.Stop();

            videoPlayer.Prepare();

            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        if (!isActive)
            return;

        player.Play();
    }

    public void MarkGestureComplete()
    {
        if (!isActive)
            return;

        isActive = false;

        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.Stop();
        }

        OnGestureCompleted?.Invoke();

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.Stop();
        }

        isActive = false;
    }
}