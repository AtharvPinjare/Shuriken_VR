using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private Transform _target;

    public void SetTarget(Transform target) => _target = target;

    private void LateUpdate()
    {
        if (_target == null) return;
        Vector3 dirToTarget = (_target.position - transform.position).normalized;
        transform.LookAt(transform.position - dirToTarget, Vector3.up);
    }
}