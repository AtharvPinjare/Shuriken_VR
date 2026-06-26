using System;
using UnityEngine;

public class HandTrackingGestureProvider : MonoBehaviour, IGestureProvider
{
    public event Action OnFireballGesture;

    // Called from Inspector — wire SelectorUnityEventWrapper.WhenSelected to this
    public void NotifyFireballGesture()
    {
        OnFireballGesture?.Invoke();
    }
}
