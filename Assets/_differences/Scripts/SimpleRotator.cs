using UnityEngine;

public class SimpleRotator : MonoBehaviour {
    [SerializeField] Vector3 _rotation = default;
    [SerializeField] float _speed = 1;
    [SerializeField] Space _space = default;

    void Update() {
        transform.Rotate(_rotation * Time.deltaTime * _speed, _space);
    }
}