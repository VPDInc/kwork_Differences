using Airion.Audio;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

[RequireComponent(typeof(Button))]
public class UIPlaySoundOnButtonClick : MonoBehaviour {
    [SerializeField] string _soundName = "OpenClick";
    
    [Inject] AudioManager _audioManager = default;

    void Start() {
        GetComponent<Button>().onClick.AddListener(() => _audioManager.PlayOnce(_soundName));
    }
}
