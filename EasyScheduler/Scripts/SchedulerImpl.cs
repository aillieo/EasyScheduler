// -----------------------------------------------------------------------
// <copyright file="SchedulerImpl.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.Assertions;

    [DefaultExecutionOrder(-100)]
    internal class SchedulerImpl : SingletonMonoBehaviour<SchedulerImpl>
    {
        // events
        internal readonly Event earlyUpdate = new Event();
        internal readonly Event fixedUpdate = new Event();
        internal readonly Event preUpdate = new Event();
        internal readonly Event update = new Event();
        internal readonly Event preLateUpdate = new Event();
        internal readonly Event lateUpdate = new Event();
        internal readonly Event postLateUpdate = new Event();

        // delay
        internal readonly Queue<Action> delayTasks = new Queue<Action>();

        // dynamic
        internal readonly LinkedList<ScheduledTimingTaskDynamic> managedDynamicTasks = new LinkedList<ScheduledTimingTaskDynamic>();
        internal readonly LinkedList<ScheduledTimingTaskDynamic> managedDynamicTasksUnscaled = new LinkedList<ScheduledTimingTaskDynamic>();
        internal readonly List<ScheduledTimingTaskDynamic> tasksToProcessDynamic = new List<ScheduledTimingTaskDynamic>();

        // static
        internal readonly List<ScheduledTimingTaskStatic> managedStaticTasks = new List<ScheduledTimingTaskStatic>();
        internal readonly List<ScheduledTimingTaskStatic> managedStaticTasksUnscaled = new List<ScheduledTimingTaskStatic>();
        internal readonly List<ScheduledTimingTaskStatic> tasksToProcessStatic = new List<ScheduledTimingTaskStatic>();

        // dynamic
        internal readonly LinkedList<ScheduledFrameTaskDynamic> managedDynamicFrameTasks = new LinkedList<ScheduledFrameTaskDynamic>();
        internal readonly List<ScheduledFrameTaskDynamic> frameTasksToProcessDynamic = new List<ScheduledFrameTaskDynamic>();

        // static
        internal readonly List<ScheduledFrameTaskStatic> managedStaticFrameTasks = new List<ScheduledFrameTaskStatic>();
        internal readonly List<ScheduledFrameTaskStatic> frameTasksToProcessStatic = new List<ScheduledFrameTaskStatic>();

        internal readonly SynchronizationContext synchronizationContext;

        internal float globalTimeScale = 1.0f;
        internal int updatePhase;

        private static readonly Predicate<ScheduledTimingTaskStatic> removePredicateTiming = task => task.removed;
        private static readonly Predicate<ScheduledFrameTaskStatic> removePredicateFrame = task => task.removed;

        private SchedulerImpl()
        {
            // typeof(UnitySynchronizationContext)
            this.synchronizationContext = SynchronizationContext.Current;
            Assert.IsNotNull(this.synchronizationContext);
        }

        public static float GlobalTimeScale
        {
            get => Instance.globalTimeScale;
            set => Instance.globalTimeScale = value;
        }

        public static int UpdatePhase
        {
            get => Instance.updatePhase;
        }

        internal void PlayerLoopEarlyUpdate()
        {
            // early update phase
            this.updatePhase = 1;

            try
            {
                this.earlyUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            this.updatePhase = 0;
        }

        internal void PlayerLoopFixedUpdate()
        {
            this.updatePhase = 2;

            try
            {
                this.fixedUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            this.updatePhase = 0;
        }

        internal void PlayerLoopPreUpdate()
        {
            // pre update phase
            this.updatePhase = 3;

            try
            {
                this.preUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            this.updatePhase = 0;

            this.ProcessDelayTasks();
        }

        internal void PlayerLoopUpdate()
        {
            // update phase
            this.updatePhase = 4;

            try
            {
                this.update.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            this.updatePhase = 0;

            this.ProcessDelayTasks();

            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;

            if (this.globalTimeScale != 1f)
            {
                deltaTime *= this.globalTimeScale;
            }

            this.ProcessTimingTasksStatic(this.managedStaticTasks, deltaTime);
            this.ProcessTimingTasksStatic(this.managedStaticTasksUnscaled, unscaledDeltaTime);

            this.ProcessTimingTasksDynamic(this.managedDynamicTasks, deltaTime);
            this.ProcessTimingTasksDynamic(this.managedDynamicTasksUnscaled, unscaledDeltaTime);

            this.ProcessFrameTasksStatic();
            this.ProcessFrameTasksDynamic();
        }

        internal void PlayerLoopPreLateUpdate()
        {
            this.updatePhase = 5;

            try
            {
                this.preLateUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            this.updatePhase = 0;
        }

        internal void PlayerLoopLateUpdate()
        {
            this.updatePhase = 6;

            try
            {
                this.lateUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            this.updatePhase = 0;

            this.ProcessDelayTasks();
        }

        internal void PlayerLoopPostLateUpdate()
        {
            this.updatePhase = 7;

            try
            {
                this.postLateUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            this.updatePhase = 0;
        }

        protected override void Awake()
        {
            base.Awake();

            if (this == Instance)
            {
                PlayerLoopRegDef.RegisterPlayerLoops(this);
            }
        }

        protected override void OnDestroy()
        {
            if (this == Instance)
            {
                PlayerLoopRegDef.UnregisterPlayerLoops(this);
            }

            base.OnDestroy();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            CreateInstance();
        }

        private void ProcessDelayTasks()
        {
            while (this.delayTasks.Count > 0)
            {
                Action action = this.delayTasks.Dequeue();
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        private void ProcessTimingTasksDynamic(LinkedList<ScheduledTimingTaskDynamic> tasks, float delta)
        {
            this.tasksToProcessDynamic.AddRange(tasks);
            foreach (var task in this.tasksToProcessDynamic)
            {
                task.timer += delta * task.localTimeScale;
                while (task.timer > task.interval)
                {
                    task.timer -= task.interval;
                    try
                    {
                        task.action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    if (task.times > 0)
                    {
                        task.times--;
                        if (task.times == 0)
                        {
                            tasks.Remove(task.handle);
                            task.handle = null;
                            task.removed = true;
                            task.isDone = true;
                            break;
                        }
                    }
                }
            }

            this.tasksToProcessDynamic.Clear();
        }

        private void ProcessTimingTasksStatic(List<ScheduledTimingTaskStatic> tasks, float delta)
        {
            var hasAnyToRemove = false;
            this.tasksToProcessStatic.AddRange(tasks);
            foreach (var task in this.tasksToProcessStatic)
            {
                if (task.removed)
                {
                    // from Unschedule()
                    hasAnyToRemove = true;
                    continue;
                }

                task.timer += delta * task.localTimeScale;
                while (task.timer > task.interval)
                {
                    task.timer -= task.interval;
                    try
                    {
                        task.action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    if (task.times > 0)
                    {
                        task.times--;
                        if (task.times == 0)
                        {
                            task.removed = true;
                            hasAnyToRemove = true;
                            task.isDone = true;
                            break;
                        }
                    }
                }
            }

            this.tasksToProcessStatic.Clear();
            if (hasAnyToRemove)
            {
                tasks.RemoveAll(removePredicateTiming);
            }
        }

        private void ProcessFrameTasksDynamic()
        {
            this.frameTasksToProcessDynamic.AddRange(this.managedDynamicFrameTasks);
            foreach (var task in this.frameTasksToProcessDynamic)
            {
                task.counter++;
                while (task.counter >= task.frameInterval)
                {
                    task.counter -= task.frameInterval;
                    try
                    {
                        task.action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    if (task.times > 0)
                    {
                        task.times--;
                        if (task.times == 0)
                        {
                            this.managedDynamicFrameTasks.Remove(task.handle);
                            task.handle = null;
                            task.removed = true;
                            task.isDone = true;
                            break;
                        }
                    }
                }
            }

            this.frameTasksToProcessDynamic.Clear();
        }

        private void ProcessFrameTasksStatic()
        {
            var hasAnyToRemove = false;
            this.frameTasksToProcessStatic.AddRange(this.managedStaticFrameTasks);
            foreach (var task in this.frameTasksToProcessStatic)
            {
                if (task.removed)
                {
                    // from Unschedule()
                    hasAnyToRemove = true;
                    continue;
                }

                task.counter++;
                while (task.counter >= task.frameInterval)
                {
                    task.counter -= task.frameInterval;
                    try
                    {
                        task.action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    if (task.times > 0)
                    {
                        task.times--;
                        if (task.times == 0)
                        {
                            task.removed = true;
                            hasAnyToRemove = true;
                            task.isDone = true;
                            break;
                        }
                    }
                }
            }

            this.frameTasksToProcessStatic.Clear();
            if (hasAnyToRemove)
            {
                this.managedStaticFrameTasks.RemoveAll(removePredicateFrame);
            }
        }
    }
}
