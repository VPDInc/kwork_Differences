using UnityEngine;
using UnityEngine.UI;

public class ProfileIconElement : MonoBehaviour {
    [SerializeField] Image _iconImage = default;

    Sprite _sprite = default;
    int _id = default;

    public Sprite Sprite => _sprite;
    public int Id => _id;

    public void SetImage(Sprite sprite, int id) {
        _sprite = sprite;
        _iconImage.sprite = sprite;
        _id = id;
    }
}