// -----------------------------------------------------------------------
// <copyright file="Scheduler.Coroutine.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Scheduler interfaces related to Unity Coroutine.
    /// </summary>
    public static partial class Scheduler
    {
        /// <summary>
        /// Start a unity coroutine.
        /// </summary>
        /// <param name="routine">Routine to execute.</param>
        /// <returns>Coroutine started.</returns>
        public static Coroutine StartUnityCoroutine(IEnumerator routine)
        {
            return SchedulerImpl.Instance.StartCoroutine(routine);
        }

        /// <summary>
        /// Stop a unity coroutine.
        /// </summary>
        /// <param name="routine">Coroutine to stop.</param>
        public static void StopUnityCoroutine(Coroutine routine)
        {
            SchedulerImpl.Instance.StopCoroutine(routine);
        }

        /// <summary>
        ///  Stops all coroutines started by <see cref="Scheduler"/>>.
        /// </summary>
        public static void StopAllUnityCoroutines()
        {
            SchedulerImpl.Instance.StopAllCoroutines();
        }
    }
}
