using System;
using System.Collections;

using UnityEngine;

public class ImagesLoader : MonoBehaviour {
    public void LoadImagesAndCreateSprite(string path1, string path2, Action<Sprite, Sprite> Completed) {
        StartCoroutine(Loading(path1, path2, Completed));
    }

    IEnumerator Loading(string path1, string path2, Action<Sprite, Sprite> Completed) {
        var async1 = Resources.LoadAsync<Sprite>("Images/" + path1);
        var async2 = Resources.LoadAsync<Sprite>("Images/" + path2);
        yield return new WaitWhile(() => !async1.isDone && !async2.isDone);
        Completed?.Invoke((Sprite)async1.asset, (Sprite)async2.asset);
    }
}
