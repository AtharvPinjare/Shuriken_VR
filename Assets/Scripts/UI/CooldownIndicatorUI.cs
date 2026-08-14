using UnityEngine;
using UnityEngine.UI;

public class CooldownIndicatorUI : MonoBehaviour
{
    [SerializeField] private SpellCaster _spellCaster;
    [SerializeField] private Image _cooldownImage;

    private void Update()
    {
        if (_spellCaster == null || _cooldownImage == null) return;
        _cooldownImage.fillAmount = _spellCaster.CooldownProgress;
    }
}