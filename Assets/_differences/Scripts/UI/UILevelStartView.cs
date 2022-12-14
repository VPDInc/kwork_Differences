using System;

using Airion.Audio;
using Airion.Extensions;

using DG.Tweening;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UILevelStartView : MonoBehaviour {
    [Header("References")] [SerializeField]
    TMP_Text _levelLabel = default;
    [SerializeField] TMP_Text _timerLabel = default;
    [SerializeField] TMP_Text _picturesCountLabel = default;
    [SerializeField] TMP_Text _differencesCountLabel = default;
    [SerializeField] TMP_Text _playerNameLabel = default;
    [SerializeField] Image _profileIcon = default;
    [SerializeField] Transform _picturesCounterContainer = default;
    [SerializeField] Transform _differencesCounterContainer = default;

    [Header("Prefabs")] [SerializeField] GameObject _picturesCounterPrefab = default;
    [SerializeField] GameObject _differencesCounterPrefab = default;

    [Header("Settings")] [SerializeField] int _secondsTillStart = 3;
    [Header("FX")] [SerializeField] UITrailEffect _uiTrailEffectPrefab = default;
    [SerializeField] RectTransform _fxStart = default;
    [SerializeField] RectTransform _fxTarget = default;
    [SerializeField] float _pauseBetweenSpawns = 0.02f;
    [SerializeField] int _fxAmount = 10;

    [Inject] AudioManager _audioManager = default;

    int _currentTimer = 0;
    Sequence _timerSequence = default;
    UIView _currentView = default;

    const string TIMER_PREFIX = "Round starts in: ";
    const string LEVEL_NAME_PREFIX = "Level ";
    const string PICTURES_COUNTER_POSTFIX = " pictures";
    const string DIFFERENCES_COUNTER_POSTFIX = " differences in picture";

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    public void Show() {
        _currentView.Show();
    }

    public void Hide() {
        _currentView.Hide();
    }

    public void SetLevelName(int levelNum) {
        _levelLabel.text = LEVEL_NAME_PREFIX + (levelNum + 1);
    }

    public void SetPlayerName(string name) {
        _playerNameLabel.text = name;
    }

    public void SetPlayerProfileIcon(Sprite icon) {
        _profileIcon.sprite = icon;
    }

    public void StartTimer(Action action) {
        _timerSequence?.Kill();

        _currentTimer = _secondsTillStart;

        _timerLabel.text = TIMER_PREFIX + _currentTimer;

        _timerSequence = DOTween.Sequence();
        for (int i = 0; i < _secondsTillStart; i++) {
            _timerSequence.AppendInterval(1);
            _timerSequence.AppendCallback(() => {
                                              _currentTimer--;
                                              _timerLabel.text = TIMER_PREFIX + _currentTimer;
                                              _audioManager.PlayOnce("tick");
                                          });
        }

        _timerSequence.AppendCallback(() => {
                                          SetupTrailEffect(_fxStart, _fxTarget);
                                      });
        _timerSequence.AppendInterval(1 + _fxAmount * _pauseBetweenSpawns);
        _timerSequence.AppendCallback(() => {
                                          Hide();
                                          action?.Invoke();
                                      });
    }

    public void SetPicturesCount(int count) {
        _picturesCounterContainer.DestroyAllChildren();

        for (int i = 0; i < count; i++) {
            Instantiate(_picturesCounterPrefab, _picturesCounterContainer);
        }

        _picturesCountLabel.text = count + PICTURES_COUNTER_POSTFIX;
    }

    public void SetDifferencesCount(int count) {
        _differencesCounterContainer.DestroyAllChildren();

        for (int i = 0; i < count; i++) {
            Instantiate(_differencesCounterPrefab, _differencesCounterContainer);
        }

        _differencesCountLabel.text = count + DIFFERENCES_COUNTER_POSTFIX;
    }

    public void BreakAndClose() {
        _timerSequence.Kill();
        Hide();
    }
    
    void SetupTrailEffect(Transform startTransform, RectTransform targetTransform) {
        for (int i = 0; i < _fxAmount; i++) {
            var fx = Instantiate(_uiTrailEffectPrefab, startTransform);
            fx.Setup(targetTransform.position, _pauseBetweenSpawns * i);
        }
    }
}