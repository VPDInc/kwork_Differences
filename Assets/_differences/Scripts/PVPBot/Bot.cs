using System;
using System.Timers;
using _differences.Scripts.PVPBot.Other;

namespace _differences.Scripts.PVPBot
{
    public class Bot
    {
        private const int TIMER_TICK = 1000;

        private const int MIN_TIME_FIND = 20;
        private const int MAX_TIME_FIND = 45;

        private const string LOG_START = "Bot started";
        private const string LOG_FIND = "FindDifference";

        public event Action SuccessFindDifference;
        public int CountDifferences;

        private Timer _timer;
        private TimeSpan _differenceFindTime;
        private int StepFindAmount => new Random().Next(MIN_TIME_FIND, MAX_TIME_FIND);

        public void Start()
        {
            Console.WriteLine(LOG_START);

            _differenceFindTime += new TimeSpan(0, 0, StepFindAmount);

            _timer = new Timer(TIMER_TICK);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _differenceFindTime -= new TimeSpan(0, 0, 1);

            Console.WriteLine(_differenceFindTime.ToString());

            if (_differenceFindTime.Seconds == 0)
                FindDifference();
        }

        public void FindDifference()
        {
            Console.WriteLine(LOG_FIND);

            SuccessFindDifference?.Invoke();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }

    public class DifferencesData
    {
        public int CurrentID;
        public int LastDifferences;
    }
}
