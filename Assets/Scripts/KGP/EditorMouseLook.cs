using System;
using UnityEngine;

public class EditorMouseLook : MonoBehaviour
{
    [SerializeField] private float sensitivity = 2f;

    private float _pitch;
    private float _yaw;
    private void Start()
    {
        _pitch = transform.eulerAngles.x;
        _yaw = transform.eulerAngles.y;

    }

    private void Update()
    {
        if (!Input.GetMouseButton(1)) return;
        _yaw += Input.GetAxis("Mouse X") * sensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        _pitch = Mathf.Clamp(_pitch,-89f,89f);

    
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}