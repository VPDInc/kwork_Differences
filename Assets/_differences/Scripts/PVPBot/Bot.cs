using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using _differences.Scripts.Extension;
using _differences.Scripts.PVPBot.Other;
using ModestTree;

namespace _differences.Scripts.PVPBot
{
    public class Bot
    {
        #region CONST

        private const int TIMER_TICK = 1000;

        private const int MIN_TIME_FIND = 20;
        private const int MAX_TIME_FIND = 30;

        private const string LOG_START = "Bot started";
        private const string LOG_FIND = "FindDifference";

        #endregion

        private readonly TimeSpan delaySpan = new TimeSpan(0, 0, 1);

        public int CountDifferences;
        public BotDifficulty BotDifficulty;
        public bool BotReady;

        internal string NameBot => $"bot{GetHashCode()}";

        private Timer timer;
        private bool[] differencesArray;
        private TimeSpan differenceFindTime;

        private int StepFindAmount => Extensions.GetNormalDistributedValue(MIN_TIME_FIND, MAX_TIME_FIND);

        public event Action<DifferencesData> SuccessFindDifference;
        public event Action<Bot> AllFindDifferences;

        public void Start()
        {
            Logger.Log(LOG_START);

            RefreshTimeStep();
            differencesArray = new bool[CountDifferences];

            timer = new Timer(TIMER_TICK);
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        public void Stop()
        {
            Logger.Log($"Bot: {NameBot}, was stopped");

            timer.Stop();
            timer.Dispose();

            AllFindDifferences?.Invoke(this);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            differenceFindTime = differenceFindTime.Seconds > 0 ? -delaySpan : TimeSpan.Zero;

            if (differenceFindTime.Seconds == 0 && differencesArray.Any(x => x == false))
                FindDifference();

            if (differencesArray.All(x => x))
                Stop();
        }

        private void FindDifference()
        {
            Logger.Log($"{LOG_FIND} {NameBot}");

            var filterDifferences = differencesArray.Where(x => x == false).ToArray();
            var indexOfFindDifference = filterDifferences.IndexOf(filterDifferences[new Random().Next(filterDifferences.Length)]);
            
            differencesArray[indexOfFindDifference] = true;

            RefreshTimeStep();

            SuccessFindDifference?.Invoke(new DifferencesData
            {
                CurrentId = indexOfFindDifference,
                DifferencesArray = differencesArray
            });
        }

        private void RefreshTimeStep()
        {
            differenceFindTime += new TimeSpan(0, 0, StepFindAmount);
        }
    }

    public struct DifferencesData
    {
        public int CurrentId;
        public bool[] DifferencesArray;
    }
}