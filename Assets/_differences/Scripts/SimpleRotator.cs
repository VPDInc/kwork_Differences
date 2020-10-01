using System;

using UnityEngine;

public class SimpleRotator : MonoBehaviour {
    [SerializeField] Vector3 _rotation = default;
    [SerializeField] float _speed = 1;
    [SerializeField] Space _space = default;

    float _currentSpeed;
    
    void Awake() {
        _currentSpeed = _speed;
    }

    public void SetSpeed(float speed) {
        _currentSpeed = speed;
    }

    public void RestoreSpeed() {
        _currentSpeed = _speed;
    }

    void Update() {
        transform.Rotate(_rotation * (Time.deltaTime * _currentSpeed), _space);
    }
    
    
}