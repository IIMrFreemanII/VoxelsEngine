using System;
using System.Diagnostics;
using Sirenix.Utilities;
using Debug = UnityEngine.Debug;

namespace VoxelsEngine.Utils
{
    public static class TimeUtil
    {
        public static void GetExecutionTime(string message, Action callback)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            callback.Invoke();
            stopwatch.Stop();

            string text = message.IsNullOrWhitespace() ? callback.Method.Name : message;
            Debug.Log($"{text}: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}