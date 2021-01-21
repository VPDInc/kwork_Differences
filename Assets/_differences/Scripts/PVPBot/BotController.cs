using System.Collections.Generic;

namespace _differences.Scripts.PVPBot
{
    public class BotController
    {
        private List<Bot> botPull = new List<Bot>();

        public Bot GetBot()
        {
            BotBuilder botBuilder = new BotBuilder();

            var bot = botBuilder.GetBot();

            botPull.Add(bot);

            return bot;
        }

        public void StopBot(Bot bot)
        {
            bot.Stop();
            botPull.Remove(bot);
        }
    }
}
