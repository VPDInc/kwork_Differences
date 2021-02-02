using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _differences.Scripts.PVPBot
{
    internal sealed class BotFactory
    {
        private const string LOG_INIT_BOTS = "Initializate bots: {0} count";

        private readonly List<Bot> botPull = new List<Bot>();

        internal void InitializationBots()
        {
            Debug.Log(string.Format(LOG_INIT_BOTS, botPull.Count));

            foreach (var b in botPull.Where(b => b.BotReady))
                b.Start();
        }

        internal Bot GetBot()
        {
            var bot = new Bot();

            botPull.Add(bot);

            return bot;
        }

        internal void StopBot(Bot bot)
        {
            bot.Stop();
            botPull.Remove(bot);
        }
    }
}
