using System;
using UnityEngine;

public interface IGestureProvider
{
    event Action OnFireballGesture;
    event Action OnIceShardGesture;
}
