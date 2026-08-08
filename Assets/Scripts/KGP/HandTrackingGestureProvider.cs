using System;
using UnityEngine;

public class HandTrackingGestureProvider : MonoBehaviour, IGestureProvider
{
    public event Action OnFireballGesture;
    public event Action OnIceShardGesture;

    // Called from Inspector — wire SelectorUnityEventWrapper.WhenSelected to this
    public void NotifyFireballGesture()
    {
        OnFireballGesture?.Invoke();
    }
    public void NotifyIceShardGesture()
    {
        OnIceShardGesture?.Invoke();
    }
}
