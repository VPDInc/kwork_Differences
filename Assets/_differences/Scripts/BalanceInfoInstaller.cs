using UnityEngine;

using Zenject;

[CreateAssetMenu(fileName = "BalanceInfoInstaller", menuName = "Installers/BalanceInfoInstaller")]
public class BalanceInfoInstaller : ScriptableObjectInstaller<BalanceInfoInstaller> {
    public LevelBalanceLibrary LevelBalanceLibrary;

    public override void InstallBindings() {
        Container.BindInstance(LevelBalanceLibrary);
    }
}