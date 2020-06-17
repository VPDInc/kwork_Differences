using System;
using System.Collections;

using Airion.Extensions;

using UnityEngine;
using UnityEngine.AddressableAssets;

public class Loader {
    public (Sprite, Sprite) Result { get; private set; }

    Storage _storage;
    AsyncFunc _loading;
    MonoBehaviour _owner;
    string _path1;
    string _path2;

    const string RESOURCES_DATA_PATH = "Images/";
    const string ADDRESSABLE_DATA_PATH = "Assets/_differences/Addresables/Atlas.spriteatlas";


    public Loader(MonoBehaviour owner, Storage storage = Storage.Addressable) {
        _owner = owner;
        SetStorage(storage);
    }
    
    void SetStorage(Storage storage) {
        if (_loading != null)
            _loading.Stop();
        _storage = storage;
        switch (storage) {
            case Storage.Resources:
                _loading = new AsyncFunc(_owner, LoadFromResources);
                break;
            case Storage.Addressable:
                _loading = new AsyncFunc(_owner, LoadFromAddressable);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(storage), storage, null);
        }
    }

    public IEnumerator Run(string path1, string path2, Storage storage) {
        _loading.Stop();
        _path1 = path1;
        _path2 = path2;
        SetStorage(storage);
        _loading.Start();
        yield return new WaitWhile(()=>_loading.IsProcessing);
    }
    
    IEnumerator LoadFromResources() {
        var async1 = Resources.LoadAsync<Sprite>(RESOURCES_DATA_PATH + _path1);
        var async2 = Resources.LoadAsync<Sprite>(RESOURCES_DATA_PATH + _path2);
        yield return new WaitWhile(() => !async1.isDone && !async2.isDone);
        Result = ((Sprite) async1.asset, (Sprite) async2.asset);
    }
    
    IEnumerator LoadFromAddressable() {
        var async1 = Addressables.LoadAssetAsync<Sprite>($"{ADDRESSABLE_DATA_PATH}[{_path1}]");
        var async2 = Addressables.LoadAssetAsync<Sprite>($"{ADDRESSABLE_DATA_PATH}[{_path2}]");

        yield return new WaitWhile(() => !(async1.IsDone && async2.IsDone));
        Result = (async1.Result, async2.Result);
    }
}

public enum Storage {
    Resources,
    Addressable
}
