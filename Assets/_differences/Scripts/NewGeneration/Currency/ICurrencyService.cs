namespace _differences.Scripts.NewGeneration.Currency
{
    public interface ICurrencyService
    {
        /// <summary>
        /// Get all available currencys in project
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Airion.Currency.Currency GetCurrency(string type);
        /// <summary>
        /// Earn currency
        /// </summary>
        /// <param name="count">amount currency</param>
        /// <param name="type">type currency</param>
        void Earn(int count, string type);
        /// <summary>
        /// Spend currency
        /// </summary>
        /// <param name="count">amount currency</param>
        /// <param name="type">type currency</param>
        void Spend(int count, string type);
    }
}
