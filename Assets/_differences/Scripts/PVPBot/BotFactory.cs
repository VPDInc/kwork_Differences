using _differences.Scripts.Configs;
using System.Collections.Generic;

namespace _differences.Scripts.PVPBot
{
    public class BotFactory
    {
        //add prototype ??

        private List<Bot> botPull = new List<Bot>();

        public Bot GetBot()
        {
            Bot bot = new Bot();

            botPull.Add(bot);

            return bot;
        }

        public Bot GetBot(BotConfig botConfig)
        {
            Bot bot = new Bot();

            bot.SetConfig(botConfig);

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
