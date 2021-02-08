using _differences.Scripts.NewGeneration.Currency;
using _differences.Scripts.ServerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zenject;

namespace _differences.Scripts.NewGeneration
{
    public class MainInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ICurrencyService>().To<CurrencyService>().AsSingle();
            Container.Bind<IServerAPIClient>().To<PlayFabServerAPIClient>().AsSingle();
        }
    }
}
