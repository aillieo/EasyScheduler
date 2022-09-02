// -----------------------------------------------------------------------
// <copyright file="SchedulerImpl.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Concurrent;
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
        internal readonly Queue<Action>[] delayQueues = new Queue<Action>[] { new Queue<Action>(), new Queue<Action>() };

        // dynamic
        internal readonly LinkedList<ScheduledTimingTaskDynamic> managedDynamicTasks = new LinkedList<ScheduledTimingTaskDynamic>();
        internal readonly LinkedList<ScheduledTimingTaskDynamic> managedDynamicTasksUnscaled = new LinkedList<ScheduledTimingTaskDynamic>();

        // static
        internal readonly List<ScheduledTimingTaskStatic> managedStaticTasks = new List<ScheduledTimingTaskStatic>();
        internal readonly List<ScheduledTimingTaskStatic> managedStaticTasksUnscaled = new List<ScheduledTimingTaskStatic>();

        // dynamic
        internal readonly LinkedList<ScheduledFrameTaskDynamic> managedDynamicFrameTasks = new LinkedList<ScheduledFrameTaskDynamic>();

        // static
        internal readonly List<ScheduledFrameTaskStatic> managedStaticFrameTasks = new List<ScheduledFrameTaskStatic>();

        internal readonly SynchronizationContext synchronizationContext;
        internal readonly ConcurrentQueue<(Func<object>, CancellationToken)> threadedTasksQueue = new ConcurrentQueue<(Func<object>, CancellationToken)>();
        internal int threadedTasksMaxConcurrency = 8;
        internal int threadedTasksRunning = 0;

        internal float globalTimeScale = 1.0f;
        internal int updatePhase;

        private static readonly Predicate<ScheduledTimingTask> removePredicateTiming = task => task.removed;
        private static readonly Predicate<ScheduledFrameTask> removePredicateFrame = task => task.removed;

        private static readonly string errorWithCreatingStack = "{0}\n......Registered: \n{1}";

        // process buffer dynamic
        private readonly List<ScheduledTimingTaskDynamic> tasksToProcessDynamic = new List<ScheduledTimingTaskDynamic>();

        // process buffer static
        private readonly List<ScheduledTimingTaskStatic> tasksToProcessStatic = new List<ScheduledTimingTaskStatic>();

        // process buffer dynamic frame
        private readonly List<ScheduledFrameTaskDynamic> frameTasksToProcessDynamic = new List<ScheduledFrameTaskDynamic>();

        // process buffer static frame
        private readonly List<ScheduledFrameTaskStatic> frameTasksToProcessStatic = new List<ScheduledFrameTaskStatic>();

        private SchedulerImpl()
        {
            // typeof(UnitySynchronizationContext)
            this.synchronizationContext = SynchronizationContext.Current;
            Assert.IsNotNull(this.synchronizationContext);
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

            deltaTime *= this.globalTimeScale;

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
                    Debug.LogError(e);
                }
            }
        }

        private void ProcessTimingTasksDynamic(LinkedList<ScheduledTimingTaskDynamic> tasks, float delta)
        {
            var hasAnyToRemove = false;
            this.tasksToProcessDynamic.AddRange(tasks);
            foreach (var task in this.tasksToProcessDynamic)
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
                        Debug.LogErrorFormat(errorWithCreatingStack, e, task.creatingStackTrace);
#else
                        Debug.LogError(e);
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

            this.tasksToProcessDynamic.Clear();
            if (hasAnyToRemove)
            {
                tasks.RemoveAll(removePredicateTiming);
            }
        }

        private void ProcessTimingTasksStatic(List<ScheduledTimingTaskStatic> tasks, float delta)
        {
            var hasAnyToRemove = false;
            this.tasksToProcessStatic.AddRange(tasks);
            foreach (var task in this.tasksToProcessStatic)
            {
                task.timer += delta * task.localTimeScale;
                while (task.timer > task.interval)
                {
                    if (task.removed)
                    {
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
                        Debug.LogErrorFormat(errorWithCreatingStack, e, task.creatingStackTrace);
#else
                        Debug.LogError(e);
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
            var hasAnyToRemove = false;
            this.frameTasksToProcessDynamic.AddRange(this.managedDynamicFrameTasks);
            foreach (var task in this.frameTasksToProcessDynamic)
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
                        Debug.LogErrorFormat(errorWithCreatingStack, e, task.creatingStackTrace);
#else
                        Debug.LogError(e);
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

            this.frameTasksToProcessDynamic.Clear();
            if (hasAnyToRemove)
            {
                this.managedDynamicFrameTasks.RemoveAll(removePredicateFrame);
            }
        }

        private void ProcessFrameTasksStatic()
        {
            var hasAnyToRemove = false;
            this.frameTasksToProcessStatic.AddRange(this.managedStaticFrameTasks);
            foreach (var task in this.frameTasksToProcessStatic)
            {
                if (task.removed)
                {
                    hasAnyToRemove = true;
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
                        Debug.LogErrorFormat(errorWithCreatingStack, e, task.creatingStackTrace);
#else
                        Debug.LogError(e);
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

            this.frameTasksToProcessStatic.Clear();
            if (hasAnyToRemove)
            {
                this.managedStaticFrameTasks.RemoveAll(removePredicateFrame);
            }
        }
    }
}
