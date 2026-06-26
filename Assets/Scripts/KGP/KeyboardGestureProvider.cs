using System;
using UnityEngine;

public class KeyboardGestureProvider : MonoBehaviour, IGestureProvider
{
    public event Action OnFireballGesture;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            OnFireballGesture?.Invoke();
    }
}
