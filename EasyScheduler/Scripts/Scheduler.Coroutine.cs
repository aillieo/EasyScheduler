using System.Collections;
using UnityEngine;

namespace AillieoUtils
{
    public static partial class Scheduler
    {
        public static Coroutine StartUnityCoroutine(IEnumerator routine)
        {
            return SchedulerImpl.Instance.StartCoroutine(routine);
        }

        public static void StopUnityCoroutine(Coroutine routine)
        {
            SchedulerImpl.Instance.StopCoroutine(routine);
        }

        public static void StopAllUnityCoroutines()
        {
            SchedulerImpl.Instance.StopAllCoroutines();
        }
    }
}
