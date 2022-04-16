using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Assertions;
using AillieoUtils.Collections;
using System.Text;

namespace AillieoUtils
{
    [DefaultExecutionOrder(-100)]
    public partial class Scheduler : SingletonMonoBehaviour<Scheduler>
    {
        public static ScheduleMode defaultScheduleMode = ScheduleMode.Dynamic;
        public float globalTimeScale = 1.0f;

        // events
        private int updatePhase = 0;
        private readonly Event earlyUpdate = new Event();
        private readonly Event update = new Event();
        private readonly Event lateUpdate = new Event();
        private readonly Event fixedUpdate = new Event();
        // delay
        private readonly Queue<Action> delayTasks = new Queue<Action>();
        // dynamic
        private readonly LinkedList<ScheduledTaskDynamic> managedDynamicTasks = new LinkedList<ScheduledTaskDynamic>();
        private readonly LinkedList<ScheduledTaskDynamic> managedDynamicTasksUnscaled = new LinkedList<ScheduledTaskDynamic>();
        private readonly List<ScheduledTaskDynamic> tasksToProcessDynamic = new List<ScheduledTaskDynamic>();
        // static
        private readonly List<ScheduledTaskStatic> managedStaticTasks = new List<ScheduledTaskStatic>();
        private readonly List<ScheduledTaskStatic> managedStaticTasksUnscaled = new List<ScheduledTaskStatic>();
        private readonly List<ScheduledTaskStatic> tasksToProcessStatic = new List<ScheduledTaskStatic>();
        // long term
        private readonly PriorityQueue<ScheduledTaskLongTerm> managedLongTermTasks = new PriorityQueue<ScheduledTaskLongTerm>();
        private readonly PriorityQueue<ScheduledTaskLongTerm> managedLongTermTasksUnscaled = new PriorityQueue<ScheduledTaskLongTerm>();
        private float topTimer;
        private float topTimerUnscaled;

        // dynamic
        private readonly LinkedList<ScheduledFrameTaskDynamic> managedDynamicFrameTasks = new LinkedList<ScheduledFrameTaskDynamic>();
        private readonly List<ScheduledFrameTaskDynamic> frameTasksToProcessDynamic = new List<ScheduledFrameTaskDynamic>();
        // static
        private readonly List<ScheduledFrameTaskStatic> managedStaticFrameTasks = new List<ScheduledFrameTaskStatic>();
        private readonly List<ScheduledFrameTaskStatic> frameTasksToProcessStatic = new List<ScheduledFrameTaskStatic>();

        private readonly SynchronizationContext synchronizationContext;

        public Scheduler()
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

        private void Update()
        {
            ProcessDelayTasks();

            // early update phase
            updatePhase = 1;

            try
            {
                earlyUpdate.SafeInvoke();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

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

            float deltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;

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
            ProcessDelayTasks();

            updatePhase = 3;

            try
            {
                lateUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
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

        private void ProcessTimingTasksDynamic(LinkedList<ScheduledTaskDynamic> tasks, float delta)
        {
            tasksToProcessDynamic.AddRange(tasks);
            foreach (var task in tasksToProcessDynamic)
            {
                task.timer += delta * task.speedRate;
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

        private void ProcessTimingTasksStatic(List<ScheduledTaskStatic> tasks, float delta)
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

                task.timer += delta * task.speedRate;
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

        private void ProcessTimingTasksLongTerm(PriorityQueue<ScheduledTaskLongTerm> tasks, ref float topTimerRef, float delta)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            topTimerRef += delta;

            while (tasks.Count > 0 && topTimerRef > tasks.Peek().timer)
            {
                ScheduledTaskLongTerm task = tasks.Dequeue();

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

        private static void EnqueueLongTermTask(ScheduledTaskLongTerm task, PriorityQueue<ScheduledTaskLongTerm> tasks, ref float topTimerRef)
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

        public static string GetRunningInfo()
        {
            Scheduler ins = Instance;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Running Tasks:");
            stringBuilder.AppendLine($"Dynamic : {ins.managedDynamicTasks.Count + ins.managedDynamicTasksUnscaled.Count}");
            stringBuilder.AppendLine($"Static : {ins.managedStaticTasks.Count + ins.managedStaticTasksUnscaled.Count}");
            stringBuilder.AppendLine($"Long term : {ins.managedLongTermTasks.Count + ins.managedLongTermTasksUnscaled.Count}");
            stringBuilder.AppendLine($"Update : {ins.update.ListenerCount}");
            stringBuilder.AppendLine($"LateUpdate : {ins.lateUpdate.ListenerCount}");
            stringBuilder.AppendLine($"FixedUpdate : {ins.fixedUpdate.ListenerCount}");
            stringBuilder.AppendLine($"DelayTasks : {ins.delayTasks.Count}");
            return stringBuilder.ToString();
        }
    }
}
