using Airion.Audio;

using DG.Tweening;

using UnityEngine;

using Zenject;

public class ThemeController : MonoBehaviour {
    [Inject] AudioManager _audioManager = default;

    const string MENU_THEME_ID = "menu-theme";
    const string GAMEPLAY_THEME_ID = "gameplay-theme";

    AudioPlayer _managedPlayer;
    
    void Start() {
        PlayMainTheme();
    }

    public void PlayMainTheme() {
        SwitchTheme(MENU_THEME_ID);
    }

    public void PlayGameplayTheme() {
        SwitchTheme(GAMEPLAY_THEME_ID);
    }

    public void StopTheme() {
        Stop();
    }

    void SwitchTheme(string themeID) {
        DOTween.Kill(this, true);
        var seq = DOTween.Sequence().SetId(this);
        if (_managedPlayer != null) {
            seq.AppendCallback(Stop);
            seq.AppendInterval(1);
        }

        seq.AppendCallback(() => {
            _managedPlayer = _audioManager.TryGetSoundManaged(themeID);
            _managedPlayer.SmoothPlay(0, true, 0.5f);
        });
    }

    void Stop() {
        if (_managedPlayer != null) {
            var sound = _managedPlayer;
            sound.SmoothStop(1);
            sound.Delete(1.1f);
            _managedPlayer = null;
        }
    }
}
