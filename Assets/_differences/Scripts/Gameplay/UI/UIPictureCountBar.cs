using System.Collections.Generic;

using Airion.Extensions;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class UIPictureCountBar : MonoBehaviour {
    [SerializeField] int _initialPicturesSpots = 0;
    [SerializeField] float _delta = 1;
    [SerializeField] float _startSize = 100;
    [SerializeField] RectTransform _back = default;
    [SerializeField] RectTransform _content = default;
    [SerializeField] GameObject _singleObject = default;
    [SerializeField] GameObject _multiObject = default;

    [SerializeField] GameObject _leftPointPrefab = default;
    [SerializeField] GameObject _centerPointPrefab = default;
    [SerializeField] GameObject _rightPointPrefab = default;

    int _maxPoints;
    int _currentPoints;

    public void SetSegmentAmount(int amount) {
        _maxPoints = amount;
        _currentPoints = 0;
        DOTween.Kill(this);
        _content.DestroyAllChildren();

        if (amount == 1) {
            _singleObject.SetActive(true);
            _multiObject.SetActive(false);
        } else {
            _singleObject.SetActive(false);
            _multiObject.SetActive(true);

            _back.sizeDelta = new Vector2(_startSize + _delta * (amount - _initialPicturesSpots), _back.sizeDelta.y);
        }
    }

    public void AddSegment() {
        GameObject segmentPrefab = null;

        if (_currentPoints == 0) {
            segmentPrefab = _leftPointPrefab;
        } else if (_currentPoints == _maxPoints - 1) {
            segmentPrefab = _rightPointPrefab;
        } else {
            segmentPrefab = _centerPointPrefab;
        }

        var segment = Instantiate(segmentPrefab, _content).GetComponent<Image>();
        segment.fillAmount = 0;
        segment.DOFillAmount(1, 1).SetId(this);
        _currentPoints++;
    }
}