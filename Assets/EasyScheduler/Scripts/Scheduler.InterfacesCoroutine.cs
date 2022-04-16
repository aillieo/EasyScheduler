using System.Collections;
using UnityEngine;

namespace AillieoUtils
{
    public partial class Scheduler
    {
        public static Coroutine StartUnityCoroutine(IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }

        public static void StopUnityCoroutine(Coroutine routine)
        {
            Instance.StopCoroutine(routine);
        }

        public static void StopAllUnityCoroutines()
        {
            Instance.StopAllCoroutines();
        }
    }
}
