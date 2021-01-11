

using Airion.Currency;
using Zenject;

namespace _differences.Scripts.NewGeneration.Currency
{
    public class CurrencyService : ICurrencyService
    {
        [Inject] CurrencyManager _currencyManager = default;

        public void Earn(int count, string type)
        {
            throw new System.NotImplementedException();
        }

        public Airion.Currency.Currency GetCurrency(string type)
        {
            throw new System.NotImplementedException();
        }

        public void Spend(int count, string type)
        {
            throw new System.NotImplementedException();
        }
    }
}
