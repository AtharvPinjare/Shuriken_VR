using System;
using UnityEngine;

public class GestureManager : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _gestureProvider;
    private IGestureProvider _provider;

    public event Action OnFireballGesture;

    private void Awake()
    {
        _provider = _gestureProvider as IGestureProvider;
        if (_provider == null)
            Debug.LogError("GestureManager: assigned object does not implement IGestureProvider.", this);
    }

    private void OnEnable()
    {
        if (_provider != null)
            _provider.OnFireballGesture += HandleFireball;
    }

    private void OnDisable()
    {
        if (_provider != null)
            _provider.OnFireballGesture -= HandleFireball;
    }

    private void HandleFireball() => OnFireballGesture?.Invoke();

}

