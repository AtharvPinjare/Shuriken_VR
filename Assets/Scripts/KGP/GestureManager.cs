using System;
using UnityEngine;

public class GestureManager : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _gestureProvider;
    private IGestureProvider _provider;

    public event Action OnFireballGesture;
    public event Action OnIceShardGesture;

    private void Awake()
    {
        _provider = _gestureProvider as IGestureProvider;
        if (_provider == null)
            Debug.LogError("GestureManager: assigned object does not implement IGestureProvider.", this);
    }

    private void OnEnable()
    {
        if (_provider != null)
        {
            _provider.OnFireballGesture += HandleFireball;
            _provider.OnIceShardGesture += HandleIceShard;
        }
    }

    private void OnDisable()
    {
        if (_provider != null)
        {
            _provider.OnFireballGesture -= HandleFireball;
            _provider.OnIceShardGesture -= HandleIceShard;
        }
    }

    private void HandleFireball() => OnFireballGesture?.Invoke();
    private void HandleIceShard() => OnIceShardGesture?.Invoke();

}

