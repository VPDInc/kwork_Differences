using System;
using System.Linq;
using System.Timers;

namespace _differences.Scripts.PVPBot
{
    public class Bot
    {
        private const int TIMER_TICK = 1000;

        private const int MIN_TIME_FIND = 20;
        private const int MAX_TIME_FIND = 45;

        private const string LOG_START = "Bot started";
        private const string LOG_FIND = "FindDifference";

        public event Action<DifferencesData> SuccessFindDifference;
        public event Action AllFindDiffrences;
        public int CountDifferences;

        private Timer _timer;
        private TimeSpan _differenceFindTime;
        private bool[] differencesArray;
        private int StepFindAmount => new Random().Next(MIN_TIME_FIND, MAX_TIME_FIND);

        public void Start()
        {
            Console.WriteLine(LOG_START);

            _differenceFindTime += new TimeSpan(0, 0, StepFindAmount);
            differencesArray = new bool[CountDifferences];

            _timer = new Timer(TIMER_TICK);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _differenceFindTime -= new TimeSpan(0, 0, 1);

            Console.WriteLine(_differenceFindTime.ToString());

            if (_differenceFindTime.Seconds == 0 && differencesArray.Any(x=> x == false))
                FindDifference();

            if (differencesArray.All(x => x == true))
                Stop();
        }

        private void FindDifference()
        {
            Console.WriteLine(LOG_FIND);

            for (int i = 0; i < differencesArray.Length; i++)
            {
                if (differencesArray[i] != false)
                    differencesArray[i] = true;
            }

            var data = new DifferencesData
            {
                CurrentID = 1,
                DifferencesArray = differencesArray
            };

            SuccessFindDifference?.Invoke(data);
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();

            AllFindDiffrences?.Invoke();
        }
    }

    public class DifferencesData
    {
        public int CurrentID;
        public bool[] DifferencesArray;
    }
}
