using UnityEngine;

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Shuriken VR/Spell Data")]

public class SpellData : ScriptableObject
{
    public string spellName = "Fireball";
    public float damage = 25f;

    public float projectileSpeed = 15f;

    public GameObject projectilePrefab;

    public AudioClip castClip;    
    public GameObject ImpactPrefabVFX;
}
