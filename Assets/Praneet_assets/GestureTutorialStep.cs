// GestureTutorialStep.cs
using UnityEngine;
using TMPro;
using System;

public class GestureTutorialStep : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text instructionText;

    [Tooltip("Primary hand icon (e.g. right hand). Used for all gestures.")]
    public GameObject handAnimationPlaceholder;

    [Tooltip("Secondary hand icon (e.g. left hand). Only assign for two-handed " +
             "gestures like Movement — leave empty for single-hand gestures.")]
    public GameObject handAnimationPlaceholderSecondary;

    [Header("Content")]
    [TextArea]
    public string instructionMessage = "Punch and drag your hand toward yourself to move.";

   
    public event Action OnGestureCompleted;

    private bool isActive = false;

    public void BeginStep()
    {
        isActive = true;
        gameObject.SetActive(true);

        if (instructionText != null)
            instructionText.text = instructionMessage;

        if (handAnimationPlaceholder != null)
            handAnimationPlaceholder.SetActive(true);

        if (handAnimationPlaceholderSecondary != null)
            handAnimationPlaceholderSecondary.SetActive(true);
    }

    public void MarkGestureComplete()
    {
        if (!isActive) return;
        isActive = false;

        OnGestureCompleted?.Invoke();
        gameObject.SetActive(false);
    }
}