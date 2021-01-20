namespace _differences.Scripts.PVPBot
{
    public class BotBuilder : IBotBuilder
    {
        private Bot _bot = new Bot();

        public void Reset()
        {
            this._bot = new Bot();
        }

        public Bot GetBot()
        {
            Bot result = this._bot;

            this.Reset();

            return result;
        }
    }
}
