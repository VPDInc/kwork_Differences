using System;
using UnityEngine;

public class CollectFx : MonoBehaviour 
{
    public Action OnSuccess;

    [SerializeField] UITrailEffect _uiTrailEffectPrefab = default;
    [SerializeField] RectTransform _fxStart = default;
    [SerializeField] RectTransform _fxTarget = default;
    [SerializeField] float _pauseBetweenSpawns = 0.02f;
    [SerializeField] int _fxAmount = 10;
    
    public void SetupTrailEffect() {
        for (int i = 0; i < _fxAmount; i++) {
            var fx = Instantiate(_uiTrailEffectPrefab, _fxStart);
            fx.Setup(_fxTarget.position, _pauseBetweenSpawns * i, OnSuccess);
        }
    }
}