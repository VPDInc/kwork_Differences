using Airion.Extensions;

using DG.Tweening;

using UnityEngine;

using Zenject;

public class UIGameplayTipsAdviser : MonoBehaviour {
    [SerializeField] UIShineBoosterFX[] _boosterFxes = default;
    [SerializeField] float _delayAfterLastDifference = 10;

    [Inject] GameplayHandler _gameplayHandler = default;
    
    bool _isShown;

    void Start() {
        _gameplayHandler.DifferenceFound += OnDifferenceFound;
        _gameplayHandler.GameStarted += OnGameStarted;
    }

    void OnDestroy() {
        _gameplayHandler.DifferenceFound -= OnDifferenceFound;
        _gameplayHandler.GameStarted -= OnGameStarted;
    }
    
    void OnGameStarted() {
        PlanFx();
    }

    void OnDifferenceFound() {
        PlanFx();
    }

    void PlanFx() {
        DOTween.Kill(this);
        _isShown = false;
        var bonusToShine = _boosterFxes.RandomElement();
        var seq = DOTween.Sequence().SetId(this);
        seq.AppendInterval(_delayAfterLastDifference);
        seq.AppendCallback(() => bonusToShine.Play());
        seq.AppendCallback(PlanFx);
    }
}