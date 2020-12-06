using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace VoxelsEngine.Utils
{
    public static class TimeUtil
    {
        public static void GetExecutionTime(Action callback)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            callback.Invoke();
            stopwatch.Stop();
            
            Debug.Log($"{callback.Method.Name}: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}