using System;

namespace _differences.Scripts.Helpers
{
    public static class StringToEnum
    {
        public static T Convert<T>(string input, T fallbackValue) where T : struct
        {
            if (string.IsNullOrEmpty(input) == false)
            {
                if (Enum.TryParse(input, out T result) == true)
                {
                    return result;
                }
            }

            return fallbackValue;
        }
    }
}
