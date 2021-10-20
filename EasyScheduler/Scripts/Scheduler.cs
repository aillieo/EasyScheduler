using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Assertions;
using AillieoUtils.Collections;
using System.Text;
using System.Linq;

namespace AillieoUtils
{
    [DefaultExecutionOrder(-100)]
    public class Scheduler : SingletonMonoBehaviour<Scheduler>
    {
        public static ScheduleMode defaultScheduleMode = ScheduleMode.Dynamic;
        // events
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

        public static Handle ScheduleUpdate(Action action)
        {
            return Instance.update.AddListener(action);
        }

        public static bool UnscheduleUpdate(Handle handle)
        {
            return Instance.update.Remove(handle);
        }

        public static int UnscheduleUpdate(Action action)
        {
            return Instance.update.RemoveListener(action);
        }

        private void Update()
        {
            ProcessDelayTasks();

            try
            {
                update.SafeInvoke();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            ProcessTimingTasksLongTerm(managedLongTermTasks, ref topTimer, Time.deltaTime);
            ProcessTimingTasksLongTerm(managedLongTermTasksUnscaled, ref topTimerUnscaled, Time.unscaledDeltaTime);

            ProcessTimingTasksStatic(managedStaticTasks, Time.deltaTime);
            ProcessTimingTasksStatic(managedStaticTasksUnscaled, Time.unscaledDeltaTime);

            ProcessTimingTasksDynamic(managedDynamicTasks, Time.deltaTime);
            ProcessTimingTasksDynamic(managedDynamicTasksUnscaled, Time.unscaledDeltaTime);
        }

        public static Handle ScheduleLateUpdate(Action action)
        {
            return Instance.lateUpdate.AddListener(action);
        }

        public static bool UnscheduleLateUpdate(Handle handle)
        {
            return Instance.lateUpdate.Remove(handle);
        }

        public static int UnscheduleLateUpdate(Action action)
        {
            return Instance.lateUpdate.RemoveListener(action);
        }

        private void LateUpdate()
        {
            ProcessDelayTasks();

            try
            {
                lateUpdate.SafeInvoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static Handle ScheduleFixedUpdate(Action action)
        {
            return Instance.fixedUpdate.AddListener(action);
        }

        public static bool UnscheduleFixedUpdate(Handle handle)
        {
            return Instance.fixedUpdate.Remove(handle);
        }

        public static int UnscheduleFixedUpdate(Action action)
        {
            return Instance.fixedUpdate.RemoveListener(action);
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

        public static void Delay(Action action)
        {
            Instance.delayTasks.Enqueue(action);
        }

        public static void Post(Action action)
        {
            Instance.synchronizationContext.Post(_ => action(), null);
        }

        public static void Post(SendOrPostCallback callback, object arg)
        {
            Instance.synchronizationContext.Post(callback, arg);
        }

        public static void Send(Action action)
        {
            Instance.synchronizationContext.Send(_ => action(), null);
        }

        public static void Send(SendOrPostCallback callback, object arg)
        {
            Instance.synchronizationContext.Send(callback, arg);
        }

        public static ScheduledTask ScheduleOnce(Action action, float delay)
        {
            return CreateTask(action, defaultScheduleMode, 1, 0, delay, false);
        }

        public static ScheduledTask ScheduleOnce(Action action, ScheduleMode mode, float delay)
        {
            return CreateTask(action, mode, 1, 0, delay, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, delay, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, ScheduleMode mode, float interval, float delay)
        {
            return CreateTask(action, mode, -1, interval, delay, false);
        }

        public static ScheduledTask Schedule(Action action, float interval)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, 0, false);
        }

        public static ScheduledTask Schedule(Action action, ScheduleMode mode, float interval)
        {
            return CreateTask(action, mode, -1, interval, 0, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, delay, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, ScheduleMode mode, int times, float interval, float delay)
        {
            return CreateTask(action, mode, times, interval, delay, false);
        }

        public static ScheduledTask Schedule(Action action, int times ,float interval)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, 0, false);
        }

        public static ScheduledTask Schedule(Action action, ScheduleMode mode, int times, float interval)
        {
            return CreateTask(action, mode, times, interval, 0, false);
        }

        public static ScheduledTask ScheduleOnceUnscaled(Action action, float delay)
        {
            return CreateTask(action, defaultScheduleMode, 1, 0, delay, true);
        }

        public static ScheduledTask ScheduleOnceUnscaled(Action action, ScheduleMode mode, float delay)
        {
            return CreateTask(action, mode, 1, 0, delay, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, delay, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, ScheduleMode mode, float interval, float delay)
        {
            return CreateTask(action, mode, -1, interval, delay, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, float interval)
        {
            return CreateTask(action, defaultScheduleMode, -1, interval, 0, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, ScheduleMode mode, float interval)
        {
            return CreateTask(action, mode, -1, interval, 0, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, delay, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, ScheduleMode mode, int times, float interval, float delay)
        {
            return CreateTask(action, mode, times, interval, delay, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, int times, float interval)
        {
            return CreateTask(action, defaultScheduleMode, times, interval, 0, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, ScheduleMode mode, int times, float interval)
        {
            return CreateTask(action, mode, times, interval, 0, true);
        }

        private static ScheduledTask CreateTask(Action action, ScheduleMode mode, int times, float interval, float delay, bool useUnscaledTime)
        {
            switch (mode)
            {
                case ScheduleMode.Dynamic:
                    return CreateTaskDynamic(action, times, interval, delay, useUnscaledTime);
                case ScheduleMode.Static:
                    return CreateTaskStatic(action, times, interval, delay, useUnscaledTime);
                case ScheduleMode.LongTerm:
                    return CreateTaskLongTerm(action, times, interval, delay, useUnscaledTime);
            }

            return default;
        }

        private static ScheduledTaskDynamic CreateTaskDynamic(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTaskDynamic task = new ScheduledTaskDynamic() {
                action = action,
                times = times,
                interval = interval,
                timer = - delay,
            };

            if(useUnscaledTime)
            {
                task.handle = Instance.managedDynamicTasksUnscaled.AddLast(task);
            }
            else
            {
                task.handle = Instance.managedDynamicTasks.AddLast(task);
            }

            return task;
        }

        private static ScheduledTaskStatic CreateTaskStatic(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTaskStatic task = new ScheduledTaskStatic()
            {
                action = action,
                times = times,
                interval = interval,
                timer = -delay,
            };

            if (useUnscaledTime)
            {
                Instance.managedStaticTasksUnscaled.Add(task);
            }
            else
            {
                Instance.managedStaticTasks.Add(task);
            }

            return task;
        }

        private static ScheduledTaskLongTerm CreateTaskLongTerm(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTaskLongTerm task = new ScheduledTaskLongTerm()
            {
                action = action,
                times = times,
                interval = interval,
                timer = -delay,
            };

            if (useUnscaledTime)
            {
                EnqueueLongTermTask(task, Instance.managedLongTermTasksUnscaled, ref Instance.topTimerUnscaled);
            }
            else
            {
                EnqueueLongTermTask(task, Instance.managedLongTermTasks, ref Instance.topTimer);
            }

            return task;
        }

        public static bool Unschedule(ScheduledTask task)
        {
            switch (task)
            {
                case ScheduledTaskDynamic taskDynamic:
                    return Unschedule(taskDynamic);
                case ScheduledTaskLongTerm taskLongTerm:
                    return Unschedule(taskLongTerm);
                case ScheduledTaskStatic taskStatic:
                    return Unschedule(taskStatic);
            }
            return false;
        }

        public static bool Unschedule(ScheduledTaskDynamic task)
        {
            if(task == null)
            {
                return false;
            }

            if (task.handle == null || task.removed)
            {
                return false;
            }
            task.handle.List.Remove(task.handle);
            task.handle = null;
            task.removed = true;
            return true;
        }

        public static bool Unschedule(ScheduledTaskStatic task)
        {
            if (task == null)
            {
                return false;
            }

            if (task.removed)
            {
                return false;
            }

            task.removed = true;
            return true;
        }

        public static bool Unschedule(ScheduledTaskLongTerm task)
        {
            if (task == null)
            {
                return false;
            }

            if (task.removed)
            {
                return false;
            }

            task.removed = true;
            return true;
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
