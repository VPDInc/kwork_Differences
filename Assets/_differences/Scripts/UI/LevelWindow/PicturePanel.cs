using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Airion.Extensions;

using UnityEngine;

using Zenject;

public class PicturePanel : MonoBehaviour {
    [SerializeField] UIPictureLayout[] _pictureLayout = default;

    [Inject] GameplayController _gameplay = default;

    UIPictureLayout _currentLayout = default;

    void Start() {
        _gameplay.Initialized += OnInitialized;
    }

    void OnDestroy() {
        _gameplay.Initialized -= OnInitialized;
    }

    void OnInitialized(int levelNum, Data[] levelInfo) {
        int horizontal = 0;
        int vertical = 0;

        foreach (Data data in levelInfo) {
            if (data.Orientation == Orientation.Horizontal) {
                horizontal++;
            } else {
                vertical++;
            }
        }

        ChooseLayout(horizontal, vertical);
    }

    public void ChooseLayout(int horizontal, int vertical) {
        TurnOffLayouts();

        var layouts = _pictureLayout.Where(x => x.HorizontalCount == horizontal && x.VerticalCount == vertical)
                                    .ToArray();

        _currentLayout = layouts.RandomElement();
        _currentLayout.gameObject.SetActive(true);
    }

    public void FillByImages(PictureResult[] pictureResults) {
        foreach (var pictureResult in pictureResults) {
            _currentLayout.SetPicture(pictureResult.Orientation, pictureResult.Picture);
        }
    }

    void TurnOffLayouts() {
        foreach (UIPictureLayout uiPictureLayout in _pictureLayout) {
            uiPictureLayout.gameObject.SetActive(false);
        }
    }
}