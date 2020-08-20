using UnityEngine;

using Zenject;

public class Camera2DScaler : MonoBehaviour {

    [SerializeField] Vector2Int _targetResolution;
    [SerializeField] float _targetSize = 5;
    
    [Inject] readonly Camera _camera = default;

    float _targetAspect;
    float _defaultWidth;

    void Start() {
        _targetAspect = (float)_targetResolution.x / _targetResolution.y;
        _defaultWidth = _targetSize * _targetAspect;
        _camera.orthographicSize = _defaultWidth / _camera.aspect;
    }
}