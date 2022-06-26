// -----------------------------------------------------------------------
// <copyright file="Scheduler.UpdateEvents.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;

    /// <summary>
    /// Scheduler methods related to update events.
    /// </summary>
    public static partial class Scheduler
    {
        /// <summary>
        /// Schedule a task on every EarlyUpdate.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns><see cref="Handle"/> for the EarlyUpdate event.</returns>
        public static Handle ScheduleEarlyUpdate(Action action)
        {
            return SchedulerImpl.Instance.earlyUpdate.AddListener(action);
        }

        /// <summary>
        /// Unschedule the callback registered to EarlyUpdate by handle.
        /// </summary>
        /// <param name="handle"><see cref="Handle"/> to unregister.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool UnscheduleEarlyUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.earlyUpdate.Remove(handle);
        }

        /// <summary>
        /// Unschedule the callback registered to EarlyUpdate.
        /// </summary>
        /// <param name="action">Callback to unregister.</param>
        /// <returns>Unscheduled count.</returns>
        public static int UnscheduleEarlyUpdate(Action action)
        {
            return SchedulerImpl.Instance.earlyUpdate.RemoveListener(action);
        }

        /// <summary>
        /// Schedule a task on every FixedUpdate.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns><see cref="Handle"/> for the FixedUpdate event.</returns>
        public static Handle ScheduleFixedUpdate(Action action)
        {
            return SchedulerImpl.Instance.fixedUpdate.AddListener(action);
        }

        /// <summary>
        /// Unschedule the callback registered to FixedUpdate by handle.
        /// </summary>
        /// <param name="handle"><see cref="Handle"/> to unregister.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool UnscheduleFixedUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.fixedUpdate.Remove(handle);
        }

        /// <summary>
        /// Unschedule the callback registered to FixedUpdate.
        /// </summary>
        /// <param name="action">Callback to unregister.</param>
        /// <returns>Unscheduled count.</returns>
        public static int UnscheduleFixedUpdate(Action action)
        {
            return SchedulerImpl.Instance.fixedUpdate.RemoveListener(action);
        }

        /// <summary>
        /// Schedule a task on every PreUpdate.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns><see cref="Handle"/> for the PreUpdate event.</returns>
        public static Handle SchedulePreUpdate(Action action)
        {
            return SchedulerImpl.Instance.preUpdate.AddListener(action);
        }

        /// <summary>
        /// Unschedule the callback registered to PreUpdate by handle.
        /// </summary>
        /// <param name="handle"><see cref="Handle"/> to unregister.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool UnschedulePreUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.preUpdate.Remove(handle);
        }

        /// <summary>
        /// Unschedule the callback registered to PreUpdate.
        /// </summary>
        /// <param name="action">Callback to unregister.</param>
        /// <returns>Unscheduled count.</returns>
        public static int UnschedulePreUpdate(Action action)
        {
            return SchedulerImpl.Instance.preUpdate.RemoveListener(action);
        }

        /// <summary>
        /// Schedule a task on every Update.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns><see cref="Handle"/> for the Update event.</returns>
        public static Handle ScheduleUpdate(Action action)
        {
            return SchedulerImpl.Instance.update.AddListener(action);
        }

        /// <summary>
        /// Unschedule the callback registered to Update by handle.
        /// </summary>
        /// <param name="handle"><see cref="Handle"/> to unregister.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool UnscheduleUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.update.Remove(handle);
        }

        /// <summary>
        /// Unschedule the callback registered to Update.
        /// </summary>
        /// <param name="action">Callback to unregister.</param>
        /// <returns>Unscheduled count.</returns>
        public static int UnscheduleUpdate(Action action)
        {
            return SchedulerImpl.Instance.update.RemoveListener(action);
        }

        /// <summary>
        /// Schedule a task on every PreLateUpdate.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns><see cref="Handle"/> for the PreLateUpdate event.</returns>
        public static Handle SchedulePreLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.preLateUpdate.AddListener(action);
        }

        /// <summary>
        /// Unschedule the callback registered to PreLateUpdate by handle.
        /// </summary>
        /// <param name="handle"><see cref="Handle"/> to unregister.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool UnschedulePreLateUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.preLateUpdate.Remove(handle);
        }

        /// <summary>
        /// Unschedule the callback registered to PreLateUpdate.
        /// </summary>
        /// <param name="action">Callback to unregister.</param>
        /// <returns>Unscheduled count.</returns>
        public static int UnschedulePreLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.preLateUpdate.RemoveListener(action);
        }

        /// <summary>
        /// Schedule a task on every LateUpdate.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns><see cref="Handle"/> for the LateUpdate event.</returns>
        public static Handle ScheduleLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.lateUpdate.AddListener(action);
        }

        /// <summary>
        /// Unschedule the callback registered to LateUpdate by handle.
        /// </summary>
        /// <param name="handle"><see cref="Handle"/> to unregister.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool UnscheduleLateUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.lateUpdate.Remove(handle);
        }

        /// <summary>
        /// Unschedule the callback registered to LateUpdate.
        /// </summary>
        /// <param name="action">Callback to unregister.</param>
        /// <returns>Unscheduled count.</returns>
        public static int UnscheduleLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.lateUpdate.RemoveListener(action);
        }

        /// <summary>
        /// Schedule a task on every PostLateUpdate.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns><see cref="Handle"/> for the PostLateUpdate event.</returns>
        public static Handle SchedulePostLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.postLateUpdate.AddListener(action);
        }

        /// <summary>
        /// Unschedule the callback registered to PostLateUpdate by handle.
        /// </summary>
        /// <param name="handle"><see cref="Handle"/> to unregister.</param>
        /// <returns>Unschedule succeed.</returns>
        public static bool UnschedulePostLateUpdate(Handle handle)
        {
            return SchedulerImpl.Instance.postLateUpdate.Remove(handle);
        }

        /// <summary>
        /// Unschedule the callback registered to PostLateUpdate.
        /// </summary>
        /// <param name="action">Callback to unregister.</param>
        /// <returns>Unscheduled count.</returns>
        public static int UnschedulePostLateUpdate(Action action)
        {
            return SchedulerImpl.Instance.postLateUpdate.RemoveListener(action);
        }
    }
}
