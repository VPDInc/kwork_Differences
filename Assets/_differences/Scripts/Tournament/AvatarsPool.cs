using System;
using System.Collections;
using System.Collections.Generic;

using Facebook.Unity;

using UnityEngine;

using Zenject;

public class AvatarsPool : MonoBehaviour {
    [Inject] PlayerInfoController _infoController = default;
    
    readonly Dictionary<string, (bool, Sprite)> _avatarsPool = new Dictionary<string, (bool, Sprite)>();

    public void SetAvatarAsync(LeaderboardPlayer player, Action<Sprite> callback)
    {
        StartCoroutine(SetAvatarAsyncRoutine(player, callback));
    }

    IEnumerator SetAvatarAsyncRoutine(LeaderboardPlayer player, Action<Sprite> callback)
    {
        if (!_avatarsPool.ContainsKey(player.Id))
        {
            _avatarsPool.Add(player.Id, (false, null));
            if (player.IsMe)
            {
                _avatarsPool[player.Id] = (true, _infoController.PlayerIcon);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(player.Facebook))
                {
                    FB.API($"{player.Facebook}/picture?type=square&height=200&width=200", HttpMethod.GET,
                    res =>
                    {
                        var sprite = Sprite.Create(res.Texture, new Rect(0, 0, 200, 200), new Vector2());
                        _avatarsPool[player.Id] = (true, sprite);
                    });
                }
                else
                {
                    _avatarsPool[player.Id] = (true, _infoController.GetRandomIcon());
                }
            }

        }

        yield return new WaitWhile(() => !_avatarsPool[player.Id].Item1);
        callback?.Invoke(_avatarsPool[player.Id].Item2);
    }
}
