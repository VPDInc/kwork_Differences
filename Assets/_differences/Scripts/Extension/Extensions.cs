using System.Collections.Generic;

namespace _differences.Scripts.Extension
{
    public static class Extensions
    {
        private static System.Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static int GetNormalDistributedValue(int minValue, int maxValue, float diffuctultyPercent = 100, int cyclesCount = 15)
        {
            System.Random r = new System.Random();
            int result = 0;
            for (int i = 0; i <= cyclesCount; i++)
            {
                result += r.Next(18, 30);
            }
            result /= cyclesCount;
            return result;
        }
    }
}
