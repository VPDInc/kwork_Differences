using System;

using UnityEngine;

using Zenject;

public class Camera2DScaler : MonoBehaviour {

    [SerializeField] Vector2Int _targetResolution;
    [SerializeField] float _targetSize = 5;
    
    Camera _camera;

    float _targetAspect;
    float _defaultWidth;

    void Awake() {
        _camera = GetComponent<Camera>();
    }

    void Start() {
        _targetAspect = (float)_targetResolution.x / _targetResolution.y;
        _defaultWidth = _targetSize * _targetAspect;
        _camera.orthographicSize = _defaultWidth / _camera.aspect;
    }
}