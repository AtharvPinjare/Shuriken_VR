using TMPro;
using UnityEngine;

public class WaveCounterUI : MonoBehaviour
{
    [SerializeField] private WaveManager _waveManager;
    [SerializeField] private TMP_Text _waveText;

    private void OnEnable()
    {
        if (_waveManager == null) { Debug.LogError($"{name}: WaveCounterUI has no WaveManager assigned."); return; }
        _waveManager.OnWaveStarted.AddListener(UpdateWaveText);
        UpdateWaveText(_waveManager.CurrentWave); // catch up immediately in case Wave 1 already fired before we subscribed
    }

    private void OnDisable()
    {
        if (_waveManager == null) return;
        _waveManager.OnWaveStarted.RemoveListener(UpdateWaveText);
    }

    private void UpdateWaveText(int waveNumber)
    {
        _waveText.text = waveNumber >= _waveManager.TotalWaves ? "Final Wave!" : $"Wave {waveNumber}";
    }
}