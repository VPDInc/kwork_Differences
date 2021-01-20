namespace _differences.Scripts.PVPBot
{
    public class BotController
    {
        public Bot GetBot()
        {
            BotBuilder botBuilder = new BotBuilder();
            return botBuilder.GetBot();
        }
    }
}
