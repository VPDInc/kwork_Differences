using System;
using UnityEngine;

//TODO 13.01.2021 need create other FX system, THIS BULLSHIT!
public class CollectFx : MonoBehaviour 
{
    [SerializeField] UITrailEffect _uiTrailEffectPrefab = default;
    [SerializeField] RectTransform _fxStart = default;
    [SerializeField] RectTransform _fxTarget = default;
    [SerializeField] float _pauseBetweenSpawns = 0.02f;
    [SerializeField] int _fxAmount = 10;
    
    
    //TODO 13.01.2021 REFACTORING!
    public void SetupTrailEffect(Action onSuccess = null) 
    {
        var countfx = 0;

        for (int i = 0; i < _fxAmount; i++) {
            var fx = Instantiate(_uiTrailEffectPrefab, _fxStart);
            fx.Setup(_fxTarget.position, _pauseBetweenSpawns * i, delegate
            {
                ++countfx;

                if (_fxAmount == countfx)
                {
                    countfx = 0;
                    onSuccess?.Invoke();
                };
            });
        }
    }
}