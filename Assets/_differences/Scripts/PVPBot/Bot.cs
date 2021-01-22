using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace _differences.Scripts.PVPBot
{
    public class Bot
    {
        private const int TIMER_TICK = 1000;

        private const int MIN_TIME_FIND = 1;
        private const int MAX_TIME_FIND = 2;

        private const string LOG_START = "Bot started";
        private const string LOG_FIND = "FindDifference";

        public event Action<DifferencesData> SuccessFindDifference;
        public event Action<Bot> AllFindDiffrences;
        public int CountDifferences;

        private string nameBot => "Name " + this.GetHashCode();

        private Timer _timer;
        private TimeSpan _differenceFindTime;
        private bool[] differencesArray;
        private int StepFindAmount => new Random().Next(MIN_TIME_FIND, MAX_TIME_FIND);

        public void Start()
        {
            Console.WriteLine(LOG_START);
            RefreshTimeStep();
            differencesArray = new bool[CountDifferences];

            _timer = new Timer(TIMER_TICK);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _differenceFindTime -= new TimeSpan(0, 0, 1);

            if (_differenceFindTime.Seconds == 0 && differencesArray.Any(x => x == false))
                FindDifference();

            if (differencesArray.All(x => x == true))
                Stop();
        }

        private void RefreshTimeStep()
        {
            _differenceFindTime += new TimeSpan(0, 0, StepFindAmount);
        }

        //need small refactoring for var. 
        private void FindDifference()
        {
            Console.WriteLine(LOG_FIND + " " + nameBot);

            var id = -1;

            var tempArray = new List<int>();
            for (int i = 0; i < differencesArray.Length; i++)
            {
                if (differencesArray[i] == false)
                    tempArray.Add(i);
            }

            var randomNumber = tempArray[new Random().Next(tempArray.Count)];

            differencesArray[randomNumber] = true;
            id = randomNumber;

            var data = new DifferencesData
            {
                CurrentID = id,
                DifferencesArray = differencesArray
            };

            RefreshTimeStep();

            SuccessFindDifference?.Invoke(data);
        }

        public void Stop()
        {
            Console.WriteLine("Bot end " + nameBot);

            _timer.Stop();
            _timer.Dispose();

            AllFindDiffrences?.Invoke(this);
        }
    }

    public class DifferencesData
    {
        public int CurrentID;
        public bool[] DifferencesArray;
    }
}
