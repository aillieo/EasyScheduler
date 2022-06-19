using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Assertions;
using AillieoUtils.Collections;
using UnityEngine.LowLevel;

namespace AillieoUtils
{
    [DefaultExecutionOrder(-100)]
    internal class SchedulerImpl : SingletonMonoBehaviour<SchedulerImpl>
    {
        public static float GlobalTimeScale
        {
            get => Instance.globalTimeScale;
            set => Instance.globalTimeScale = value;
        }

        public static int UpdatePhase
        {
            get => Instance.updatePhase;
        }

        internal float globalTimeScale = 1.0f;

        // events
        internal int updatePhase = 0;
        internal readonly Event preUpdate = new Event();
        internal readonly Event update = new Event();
        internal readonly Event lateUpdate = new Event();
        internal readonly Event fixedUpdate = new Event();
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
        // long term
        internal readonly PriorityQueue<ScheduledTimingTaskLongTerm> managedLongTermTasks = new PriorityQueue<ScheduledTimingTaskLongTerm>();
        internal readonly PriorityQueue<ScheduledTimingTaskLongTerm> managedLongTermTasksUnscaled = new PriorityQueue<ScheduledTimingTaskLongTerm>();
        internal float topTimer;
        internal float topTimerUnscaled;

        // dynamic
        internal readonly LinkedList<ScheduledFrameTaskDynamic> managedDynamicFrameTasks = new LinkedList<ScheduledFrameTaskDynamic>();
        internal readonly List<ScheduledFrameTaskDynamic> frameTasksToProcessDynamic = new List<ScheduledFrameTaskDynamic>();
        // static
        internal readonly List<ScheduledFrameTaskStatic> managedStaticFrameTasks = new List<ScheduledFrameTaskStatic>();
        internal readonly List<ScheduledFrameTaskStatic> frameTasksToProcessStatic = new List<ScheduledFrameTaskStatic>();

        internal readonly SynchronizationContext synchronizationContext;

        private SchedulerImpl()
        {
            synchronizationContext = SynchronizationContext.Current;
            // typeof(UnitySynchronizationContext)
            Assert.IsNotNull(synchronizationContext);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            CreateInstance();
        }

        protected override void Awake()
        {
            base.Awake();

            PlayerLoopSystem defaultPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < defaultPlayerLoop.subSystemList.Length; ++i)
            {
                if (defaultPlayerLoop.subSystemList[i].type != typeof(UnityEngine.PlayerLoop.PreUpdate))
                {
                    continue;
                }

                // preUpdate
                PlayerLoopSystem preUpdateSystem = defaultPlayerLoop.subSystemList[i];
                List<PlayerLoopSystem> subSystemList = new List<PlayerLoopSystem>(preUpdateSystem.subSystemList);

                PlayerLoopSystem newPlayerLoopSystem = new PlayerLoopSystem()
                {
                    type = typeof(SchedulerImpl),
                    updateDelegate = PreUpdate,
                };

                subSystemList.Add(newPlayerLoopSystem);

                preUpdateSystem.subSystemList = subSystemList.ToArray();
                defaultPlayerLoop.subSystemList[i] = preUpdateSystem;
                PlayerLoop.SetPlayerLoop(defaultPlayerLoop);
                break;
            }
        }

        protected override void OnDestroy()
        {
            PlayerLoopSystem defaultPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < defaultPlayerLoop.subSystemList.Length; ++i)
            {
                if (defaultPlayerLoop.subSystemList[i].type != typeof(UnityEngine.PlayerLoop.PreUpdate))
                {
                    continue;
                }

                // preUpdate
                PlayerLoopSystem preUpdateSystem = defaultPlayerLoop.subSystemList[i];
                List<PlayerLoopSystem> subSystemList = new List<PlayerLoopSystem>(preUpdateSystem.subSystemList);

                int removed = subSystemList.RemoveAll(pls => pls.type == typeof(SchedulerImpl));
                if (removed > 0)
                {
                    preUpdateSystem.subSystemList = subSystemList.ToArray();
                    defaultPlayerLoop.subSystemList[i] = preUpdateSystem;
                    PlayerLoop.SetPlayerLoop(defaultPlayerLoop);
                }

                break;
            }

            base.OnDestroy();
        }

        private void PreUpdate()
        {
            // pre update phase
            updatePhase = 1;

            try
            {
                preUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            updatePhase = 0;

            ProcessDelayTasks();
        }

        private void Update()
        {
            // update phase
            updatePhase = 2;

            try
            {
                update.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            updatePhase = 0;

            ProcessDelayTasks();

            float deltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            if (globalTimeScale != 1f)
            {
                deltaTime *= globalTimeScale;
            }

            ProcessTimingTasksLongTerm(managedLongTermTasks, ref topTimer, deltaTime);
            ProcessTimingTasksLongTerm(managedLongTermTasksUnscaled, ref topTimerUnscaled, unscaledDeltaTime);

            ProcessTimingTasksStatic(managedStaticTasks, deltaTime);
            ProcessTimingTasksStatic(managedStaticTasksUnscaled, unscaledDeltaTime);

            ProcessTimingTasksDynamic(managedDynamicTasks, deltaTime);
            ProcessTimingTasksDynamic(managedDynamicTasksUnscaled, unscaledDeltaTime);

            ProcessFrameTasksStatic();
            ProcessFrameTasksDynamic();
        }

        private void LateUpdate()
        {
            updatePhase = 3;

            try
            {
                lateUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            updatePhase = 0;

            ProcessDelayTasks();
        }

        private void FixedUpdate()
        {
            try
            {
                fixedUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void ProcessDelayTasks()
        {
            while (delayTasks.Count > 0)
            {
                Action action = delayTasks.Dequeue();
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
            tasksToProcessDynamic.AddRange(tasks);
            foreach (var task in tasksToProcessDynamic)
            {
                task.timer += delta * task.localTimeScale;
                while(task.timer > task.interval)
                {
                    task.timer -= task.interval;
                    try
                    {
                        task.action.Invoke();
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e);
                    }

                    if (task.times > 0)
                    {
                        task.times--;
                        if(task.times == 0)
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
            tasksToProcessDynamic.Clear();
        }

        private void ProcessTimingTasksStatic(List<ScheduledTimingTaskStatic> tasks, float delta)
        {
            bool hasAnyToRemove = false;
            tasksToProcessStatic.AddRange(tasks);
            foreach (var task in tasksToProcessStatic)
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
            tasksToProcessStatic.Clear();
            if (hasAnyToRemove)
            {
                tasks.RemoveAll(tsk => tsk.removed);
                hasAnyToRemove = false;
            }
        }

        private void ProcessTimingTasksLongTerm(PriorityQueue<ScheduledTimingTaskLongTerm> tasks, ref float topTimerRef, float delta)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            topTimerRef += delta;

            while (tasks.Count > 0 && topTimerRef > tasks.Peek().timer)
            {
                ScheduledTimingTaskLongTerm task = tasks.Dequeue();

                if (task.removed)
                {
                    continue;
                }

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
                        task.isDone = true;
                    }
                }

                if (!task.removed)
                {
                    task.timer -= task.interval;
                    EnqueueLongTermTask(task, tasks, ref topTimerRef);
                }
            }

            if (tasks.Count == 0)
            {
                topTimerRef = 0;
            }
        }

        internal static void EnqueueLongTermTask(ScheduledTimingTaskLongTerm task, PriorityQueue<ScheduledTimingTaskLongTerm> tasks, ref float topTimerRef)
        {
            if (topTimerRef > 0)
            {
                foreach (var t in tasks)
                {
                    t.timer += topTimerRef;
                }

                topTimerRef = 0;
            }

            topTimerRef = task.interval - task.timer;
            tasks.Enqueue(task);
        }

        private void ProcessFrameTasksDynamic()
        {
            frameTasksToProcessDynamic.AddRange(managedDynamicFrameTasks);
            foreach (var task in frameTasksToProcessDynamic)
            {
                task.counter ++;
                while (task.counter >= task.frameInterval)
                {
                    task.counter --;
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
                            managedDynamicFrameTasks.Remove(task.handle);
                            task.handle = null;
                            task.removed = true;
                            task.isDone = true;
                            break;
                        }
                    }
                }
            }
            frameTasksToProcessDynamic.Clear();
        }

        private void ProcessFrameTasksStatic()
        {
            bool hasAnyToRemove = false;
            frameTasksToProcessStatic.AddRange(managedStaticFrameTasks);
            foreach (var task in frameTasksToProcessStatic)
            {
                if (task.removed)
                {
                    // from Unschedule()
                    hasAnyToRemove = true;
                    continue;
                }

                task.counter ++;
                while (task.counter >= task.frameInterval)
                {
                    task.counter --;
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
            frameTasksToProcessStatic.Clear();
            if (hasAnyToRemove)
            {
                managedStaticFrameTasks.RemoveAll(tsk => tsk.removed);
                hasAnyToRemove = false;
            }
        }
    }
}
