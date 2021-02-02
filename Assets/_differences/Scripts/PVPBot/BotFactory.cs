using _differences.Scripts.Configs;
using System.Collections.Generic;

namespace _differences.Scripts.PVPBot
{
    public class BotFactory
    {
        private List<Bot> botPull = new List<Bot>();

        public void StartGame()
        {
            for (int i = 0; i < botPull.Count; i++)
            {
                if(botPull[i].BotReady)
                    botPull[i].Start();
            }
        }

        public Bot GetBot()
        {
            Bot bot = new Bot();

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
