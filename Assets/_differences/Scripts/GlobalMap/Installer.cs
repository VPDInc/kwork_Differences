using UnityEngine;

using Zenject;

[CreateAssetMenu(fileName = "BalanceInfoInstaller", menuName = "Installers/BalanceInfoInstaller")]
public class Installer : ScriptableObjectInstaller<Installer> {
    public LevelBalanceLibrary LevelBalanceLibrary;
    public TournamentRewards TournamentRewards;

    public override void InstallBindings() {
        Container.BindInstance(LevelBalanceLibrary);
        Container.BindInstance(TournamentRewards);
    }
}