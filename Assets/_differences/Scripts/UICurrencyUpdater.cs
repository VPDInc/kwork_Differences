using System;

using Airion.Currency;

using DG.Tweening;

using TMPro;

using UnityEngine;

using Zenject;

public class UICurrencyUpdater : MonoBehaviour {
    [SerializeField] float _duration = 0.75f;
    [SerializeField] float _strength = 7;
    [SerializeField] int _vibrato = 15;
    [SerializeField] float _randomness = 15;
    [SerializeField] bool _snapping = true;
    [SerializeField] bool _fadeOut = true;
    
    [SerializeField] TextMeshProUGUI _currencyText = default;
    [SerializeField] string _currencyId = default;
    [SerializeField] float _updatingDuration = 0.5f;
    [SerializeField] float _scaleChangeDuration = 0.1f;
    [SerializeField] float _scaleUp = 1.5f;
    
    [Inject] CurrencyManager _currencyManager = default;
    
    Currency _currency;
    Vector3 _startScale;
    Tween _shakeTween;
    RectTransform _rectTransform;
    Vector3 _startPos;

    void Awake() {
        _rectTransform = GetComponent<RectTransform>();
        _startPos = _rectTransform.anchoredPosition;
    }

    void Start() {
        _currency = _currencyManager.GetCurrency(_currencyId);
        _startScale = _currencyText.transform.localScale;
        _currency.Updated += OnCurrencyUpdated;
        
        UpdateText(true);
    }

    void OnDestroy() {
        _currency.Updated -= OnCurrencyUpdated;
    }

    public void Shake() {
        if (_shakeTween != null) {
            _shakeTween.Kill();
            _rectTransform.anchoredPosition = _startPos;
        }

        _shakeTween = transform.DOShakePosition(_duration, _strength, _vibrato, _randomness, _snapping, _fadeOut).SetEase(Ease.Linear);
    }

    void OnDisable() {
        transform.GetComponent<RectTransform>().DOKill(true);
    }

    void UpdateText(bool fast = false) {
        DOTween.Kill(this, true);
        
        if (fast) {
            _currencyText.text = _currency.Amount.ToString("F0");
            return;
        }

        var amount = float.Parse(_currencyText.text);
        var seq = DOTween.Sequence().SetId(this);
        seq.Append(_currencyText.transform.DOScale(_startScale * _scaleUp, _scaleChangeDuration));
        
        seq.Append(DOTween.To(() => amount, 
                                (newVal) => {
                                    amount = newVal;
                                    _currencyText.text = amount.ToString("F0");
                                }, 
                                _currency.Amount, 
                                _updatingDuration));
        
        seq.Append(_currencyText.transform.DOScale(_startScale, _scaleChangeDuration));
    }
    
    void OnCurrencyUpdated(float delta) {
        UpdateText();
    }
}
