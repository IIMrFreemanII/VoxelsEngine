using System;
using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class TimeUtil
    {
        public static void GetExecutionTime(Action callback)
        {
            float startTimeMs = Time.realtimeSinceStartup * 1000;

            callback.Invoke();

            float finishTime = Time.realtimeSinceStartup * 1000;
            Debug.Log($"{callback.Method.Name}: {finishTime - startTimeMs}ms");
        }
    }
}