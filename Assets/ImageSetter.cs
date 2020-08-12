﻿using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class ImageSetter : MonoBehaviour {
    public CanvasGroup Group => _group;
    
    [SerializeField] Image _image1 = default;
    [SerializeField] Image _image2 = default;
    [SerializeField] CanvasGroup _group = default;

    public void Set(Sprite sp1, Sprite sp2) {
        _image1.sprite = sp1;
        _image2.sprite = sp2;
    }

    public (Sprite, Sprite) Get() {
        return (_image1.sprite, _image2.sprite);
    }
    
    public (Image, Image) GetImages() {
        return (_image1, _image2);
    }

    public void Show(bool fast = false) {
        if (fast) {
            _group.alpha = 1;
            return;
        }

        _group.DOFade(1, 1);
    }

    public void Hide(bool fast = false) {
        if (fast) {
            _group.alpha = 0;
            return;
        }

        _group.DOFade(0, 1);
    }
}
