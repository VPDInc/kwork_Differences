using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

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
    [SerializeField] Button _saveButton = default;

    [Inject] PlayerInfoController _playerInfoController = default;

    int _iconId = 0;
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
    
    public void ScrollEnd() {
        var closestPicture = GetClosestPicture();
        
        SnapTo(closestPicture.GetComponent<RectTransform>(), false);
    }

    public void EndEdit(string str) {
        _saveButton.interactable = str.Length >= 3;
        if (str.Length > 15) {
            _inputField.text = str.Substring(0, 15);
        }
    }

    public void OnEditChanged(string str) {
        if (str.Length > 15) {
            _inputField.text = str.Substring(0, 15);
        }
    }

    public void HandleScroll() {
        var closestPicture = GetClosestPicture();
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
    
    public void Save() {
        _playerInfoController.PlayerName = _inputField.text;
        _playerInfoController.SetIcon(_iconId);
        Hide();
    }

    void OnInfoUpdated() {
        _inputField.text = _playerInfoController.PlayerName;
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

    ProfileIconElement GetClosestPicture() {
        var point = _selectedProfileIcon.transform.position;
        var ordered = _iconElements.OrderBy(build => Vector2.Distance(point, build.transform.position));
        var closestPicture = ordered.First();
        return closestPicture;
    }

    void SnapToActualIcon() {
        SnapTo(_iconElements[_playerInfoController.IconId].GetComponent<RectTransform>());
    }

    void SnapTo(RectTransform target, bool isInstant = true)
    {
        Canvas.ForceUpdateCanvases();
        
        var targetPos = (Vector2) _scrollRect.transform.InverseTransformPoint(_scrollRect.content.position)
                        - (Vector2) _scrollRect.transform.InverseTransformPoint(target.position);

        if (isInstant) {
            _scrollRect.content.anchoredPosition = targetPos;
        } else {
            _scrollRect.content.DOAnchorPos(targetPos, 0.25f);
        }
    }
}