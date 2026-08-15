using UnityEngine;

public class Boom : MonoBehaviour
{
    [SerializeField] private int count = 30;
    [SerializeField] private ParticleSystem explosionParticles;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) explosionParticles.Emit(count);
    }
}
