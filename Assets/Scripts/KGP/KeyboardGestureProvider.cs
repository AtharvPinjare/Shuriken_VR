using System;
using UnityEngine;

public class KeyboardGestureProvider : MonoBehaviour, IGestureProvider
{
    public event Action OnFireballGesture;
    public event Action OnIceShardGesture;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            OnFireballGesture?.Invoke();

        if (Input.GetKeyDown(KeyCode.I))
            OnIceShardGesture?.Invoke();
    }
}
