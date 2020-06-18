using System.Collections.Generic;
using System.Linq;

using Doozy.Engine.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

using Zenject;

public class UIPlayerInfoChange : MonoBehaviour {
    [SerializeField] ProfileIconElement _iconElementPrefab = default;
    [SerializeField] Transform _iconElementsHolder = default;
    [SerializeField] TMP_InputField _inputField = default;
    [SerializeField] UI_InfiniteScroll _uiInfiniteScroll = default;
    [SerializeField] Image _selectedProfileIcon = default;
    [SerializeField] ScrollRect _scrollRect = default;

    [Inject] PlayerInfoController _playerInfoController = default;

    int _iconId = 0;
    float _scrollStep = 0;
    UIView _currentView = default;
    List<ProfileIconElement> _iconElements = new List<ProfileIconElement>();

    void Awake() {
        _currentView = GetComponent<UIView>();
    }

    void Start() {
        FillImages();
        _inputField.text = _playerInfoController.PlayerName;
        
        _playerInfoController.InfoUpdated += OnInfoUpdated;
        HandleScroll();
    }

    void OnDestroy() {
        _playerInfoController.InfoUpdated -= OnInfoUpdated;
    }

    public void OnInfoUpdated() {
        _inputField.text = _playerInfoController.PlayerName;
    }

    public void HandleScroll() {
        var point = _selectedProfileIcon.transform.position;
        var ordered = _iconElements.OrderBy(build => Vector2.Distance(point, build.transform.position));
        var closestPicture = ordered.First();
        _selectedProfileIcon.sprite = closestPicture.Sprite;
        _iconId = closestPicture.Id;
    }

    public void Show() {
        _currentView.Show();
        SnapToActualIcon();
    }

    public void Hide() {
        _currentView.Hide();
    }

    void FillImages() {
        for (var i = 0; i < _playerInfoController.ProfileIcons.Length; i++) {
            Sprite profileIcon = _playerInfoController.ProfileIcons[i];
            var element = Instantiate(_iconElementPrefab, _iconElementsHolder);
            _iconElements.Add(element);
            element.SetImage(profileIcon, i);
        }
        _uiInfiniteScroll.Init();
    }

    public void Save() {
        _playerInfoController.PlayerName = _inputField.text;
        _playerInfoController.SetIcon(_iconId);
        Hide();
    }

    void SnapToActualIcon() {
        SnapTo(_iconElements[_playerInfoController.IconId].GetComponent<RectTransform>());
    }

    void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        _scrollRect.content.anchoredPosition =
            (Vector2)_scrollRect.transform.InverseTransformPoint(_scrollRect.content.position)
            - (Vector2)_scrollRect.transform.InverseTransformPoint(target.position);
    }
}