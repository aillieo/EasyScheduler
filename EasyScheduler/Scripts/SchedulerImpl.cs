// -----------------------------------------------------------------------
// <copyright file="SchedulerImpl.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Assertions;

    [DefaultExecutionOrder(-100)]
    internal class SchedulerImpl : SingletonMonoBehaviour<SchedulerImpl>
    {
        // events
        internal readonly EasyDelegate earlyUpdate = new EasyDelegate();
        internal readonly EasyDelegate fixedUpdate = new EasyDelegate();
        internal readonly EasyDelegate preUpdate = new EasyDelegate();
        internal readonly EasyDelegate update = new EasyDelegate();
        internal readonly EasyDelegate preLateUpdate = new EasyDelegate();
        internal readonly EasyDelegate lateUpdate = new EasyDelegate();
        internal readonly EasyDelegate postLateUpdate = new EasyDelegate();

        // delay
        internal readonly Queue<Action>[] delayQueues = new Queue<Action>[] { new Queue<Action>(), new Queue<Action>() };

        // dynamic
        internal readonly LinkedList<ScheduledTimingTask> managedTasks = new LinkedList<ScheduledTimingTask>();
        internal readonly LinkedList<ScheduledTimingTask> managedTasksUnscaled = new LinkedList<ScheduledTimingTask>();

        // dynamic
        internal readonly LinkedList<ScheduledFrameTask> managedFrameTasks = new LinkedList<ScheduledFrameTask>();

        internal readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        internal TaskFactory<object> taskFactory;
        internal int threadedTasksMaxConcurrency = 8;

        internal float globalTimeScale = 1.0f;
        internal int updatePhase;

        private static readonly Predicate<ScheduledTimingTask> removePredicateTiming = task => task.removed;
        private static readonly Predicate<ScheduledFrameTask> removePredicateFrame = task => task.removed;

        // process buffer dynamic
        private readonly List<ScheduledTimingTask> tasksToProcess = new List<ScheduledTimingTask>();

        // process buffer dynamic frame
        private readonly List<ScheduledFrameTask> frameTasksToProcess = new List<ScheduledFrameTask>();

        internal void PlayerLoopEarlyUpdate()
        {
            // early update phase
            this.updatePhase = 1;

            try
            {
                this.earlyUpdate.InvokeAll();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            this.updatePhase = 0;
        }

        internal void PlayerLoopFixedUpdate()
        {
            this.updatePhase = 2;

            try
            {
                this.fixedUpdate.InvokeAll();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            this.updatePhase = 0;
        }

        internal void PlayerLoopPreUpdate()
        {
            // pre update phase
            this.updatePhase = 3;

            try
            {
                this.preUpdate.InvokeAll();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
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
                this.update.InvokeAll();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            this.updatePhase = 0;

            this.ProcessDelayTasks();

            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;

            deltaTime *= this.globalTimeScale;

            this.ProcessTimingTasks(this.managedTasks, deltaTime);
            this.ProcessTimingTasks(this.managedTasksUnscaled, unscaledDeltaTime);

            this.ProcessFrameTasks();
        }

        internal void PlayerLoopPreLateUpdate()
        {
            this.updatePhase = 5;

            try
            {
                this.preLateUpdate.InvokeAll();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            this.updatePhase = 0;
        }

        internal void PlayerLoopLateUpdate()
        {
            this.updatePhase = 6;

            try
            {
                this.lateUpdate.InvokeAll();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            this.updatePhase = 0;

            this.ProcessDelayTasks();
        }

        internal void PlayerLoopPostLateUpdate()
        {
            this.updatePhase = 7;

            try
            {
                this.postLateUpdate.InvokeAll();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            this.updatePhase = 0;
        }

        protected override void Awake()
        {
            base.Awake();

            Assert.IsNotNull(this.synchronizationContext);

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

                if (this.taskFactory != null && this.taskFactory.Scheduler is SimpleTaskScheduler simpleTaskScheduler)
                {
                    simpleTaskScheduler.Dispose();
                    this.taskFactory = null;
                }
            }

            base.OnDestroy();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            CreateInstance();
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private static string FormatErrorWithStack(Exception exception, StackTrace creatingStackTrace)
        {
            return $"{exception}\n......Registered: \n{creatingStackTrace}";
        }
#endif

        private void ProcessDelayTasks()
        {
            Queue<Action> toProcess = this.delayQueues[1];
            Queue<Action> backBuffer = this.delayQueues[0];
            this.delayQueues[0] = toProcess;
            this.delayQueues[1] = backBuffer;

            while (toProcess.Count > 0)
            {
                Action action = toProcess.Dequeue();
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        private void ProcessTimingTasks(LinkedList<ScheduledTimingTask> tasks, float delta)
        {
            var hasAnyToRemove = false;
            this.tasksToProcess.AddRange(tasks);
            foreach (var task in this.tasksToProcess)
            {
                task.timer += delta * task.localTimeScale;
                while (task.timer > task.interval)
                {
                    if (task.removed)
                    {
                        task.handle = null;
                        hasAnyToRemove = true;
                        break;
                    }

                    task.timer -= task.interval;
                    try
                    {
                        task.action.Invoke();
                    }
                    catch (Exception e)
                    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        UnityEngine.Debug.LogError(FormatErrorWithStack(e, task.creatingStackTrace));
#else
                        UnityEngine.Debug.LogException(e);
#endif
                    }

                    if (task.times > 0)
                    {
                        task.times--;
                        if (task.times == 0)
                        {
                            task.isDone = true;
                            task.removed = true;
                            task.handle = null;
                            hasAnyToRemove = true;
                            break;
                        }
                    }
                }
            }

            this.tasksToProcess.Clear();
            if (hasAnyToRemove)
            {
                tasks.RemoveAll(removePredicateTiming);
            }
        }

        private void ProcessFrameTasks()
        {
            var hasAnyToRemove = false;
            this.frameTasksToProcess.AddRange(this.managedFrameTasks);
            foreach (var task in this.frameTasksToProcess)
            {
                if (task.removed)
                {
                    continue;
                }

                task.counter++;
                if (task.counter >= task.frameInterval)
                {
                    task.counter -= task.frameInterval;
                    try
                    {
                        task.action.Invoke();
                    }
                    catch (Exception e)
                    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        UnityEngine.Debug.LogError(FormatErrorWithStack(e, task.creatingStackTrace));
#else
                        UnityEngine.Debug.LogException(e);
#endif
                    }

                    if (task.times > 0)
                    {
                        task.times--;
                        if (task.times == 0)
                        {
                            task.isDone = true;
                            task.removed = true;
                            hasAnyToRemove = true;
                        }
                    }
                }
            }

            this.frameTasksToProcess.Clear();
            if (hasAnyToRemove)
            {
                this.managedFrameTasks.RemoveAll(removePredicateFrame);
            }
        }
    }
}
