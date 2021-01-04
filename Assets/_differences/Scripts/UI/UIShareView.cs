using System;
using System.Collections;

using Airion.Extensions;

using EasyMobile;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

public class UIShareView : MonoBehaviour {
    [SerializeField] string _shareText = "Try to find differences! Play now! https://apps.apple.com/ru/app/findscapes/id1530032494.";
    [SerializeField] Camera _camera = default;

    [SerializeField] CanvasGroup _horizontalGroup = default;
    [SerializeField] CanvasGroup _verticalGroup = default;

    [SerializeField] Image _horizontalImage1 = default;
    [SerializeField] Image _horizontalImage2 = default;
    [SerializeField] Image _verticalImage1 = default;
    [SerializeField] Image _verticalImage2 = default;

    [Inject] GameplayController _gameplayController = default;

    void Start() {
        _gameplayController.Completed += OnCompleted; 
    }

    void OnDestroy() {
        _gameplayController.Completed -= OnCompleted; 
    }

    void OnCompleted(GameplayResult result) {
        var randomImage = result.PictureResults.RandomElement();
        SetImages(randomImage.Orientation, randomImage.Picture1, randomImage.Picture2);
    }

    void SetImages(Orientation orientation, Sprite sprite1, Sprite sprite2) {
        switch (orientation) {
            case Orientation.Vertical:
                _verticalGroup.alpha = 1;
                _horizontalGroup.alpha = 0;
                _verticalImage1.sprite = sprite1;
                _verticalImage2.sprite = sprite2;
                break;
            case Orientation.Horizontal:
                _verticalGroup.alpha = 0;
                _horizontalGroup.alpha = 1;
                _horizontalImage1.sprite = sprite1;
                _horizontalImage2.sprite = sprite2;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
        }
    }

    [ContextMenu("Share")]
    public void ShareScreenshot() {
        StartCoroutine(SaveAndShareScreenshot());
    }

    IEnumerator SaveAndShareScreenshot() {
        // Wait until the end of frame
        yield return new WaitForEndOfFrame();
        // The SaveScreenshot() method returns the path of the saved image
        // The provided file name will be added a ".png" extension automatically
        _camera.Render();
        string path = Sharing.SaveScreenshot("screenshot");
        Sharing.ShareImage(path, _shareText);
    }

}