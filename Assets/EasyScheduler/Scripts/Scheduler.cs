using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine.Assertions;

namespace AillieoUtils
{
    [DefaultExecutionOrder(-100)]
    public class Scheduler : SingletonMonoBehaviour<Scheduler>
    {
        private readonly Event update = new Event();
        private readonly Event lateUpdate = new Event();
        private readonly Event fixedUpdate = new Event();
        private readonly Queue<Action> delayTasks = new Queue<Action>();
        private readonly LinkedList<ScheduledTask> managedTasks = new LinkedList<ScheduledTask>();
        private readonly LinkedList<ScheduledTask> managedTaskUnscaled = new LinkedList<ScheduledTask>();
        
        private readonly List<ScheduledTask> tasksToProcess = new List<ScheduledTask>();
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

            ProcessTimingTasks(managedTasks, Time.deltaTime);
            ProcessTimingTasks(managedTaskUnscaled, Time.unscaledDeltaTime);
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
            return CreateTask(action, 1, 0, delay, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, float interval, float delay)
        {
            return CreateTask(action, -1, interval, delay, false);
        }

        public static ScheduledTask Schedule(Action action, float interval)
        {
            return CreateTask(action, -1, interval, 0, false);
        }

        public static ScheduledTask ScheduleWithDelay(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, times, interval, delay, false);
        }

        public static ScheduledTask Schedule(Action action, int times ,float interval)
        {
            return CreateTask(action, times, interval, 0, false);
        }

        public static ScheduledTask ScheduleOnceUnscaled(Action action, float delay)
        {
            return CreateTask(action, 1, 0, delay, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, float interval, float delay)
        {
            return CreateTask(action, -1, interval, delay, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, float interval)
        {
            return CreateTask(action, -1, interval, 0, true);
        }

        public static ScheduledTask ScheduleWithDelayUnscaled(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, times, interval, delay, true);
        }

        public static ScheduledTask ScheduleUnscaled(Action action, int times, float interval)
        {
            return CreateTask(action, times, interval, 0, true);
        }

        private static ScheduledTask CreateTask(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            ScheduledTask task = new ScheduledTask() {
                action = action,
                times = times,
                interval = interval,
                timer = - delay,
            };

            if(useUnscaledTime)
            {
                task.handle = Instance.managedTaskUnscaled.AddLast(task);
            }
            else
            {
                task.handle = Instance.managedTasks.AddLast(task);
            }

            return task;
        }

        public static bool Unschedule(ScheduledTask task)
        {
            if(task == null || task.handle == null)
            {
                return false;
            }

            task.handle.List.Remove(task.handle);
            task.handle = null;
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

        private void ProcessTimingTasks(LinkedList<ScheduledTask> tasks, float delta)
        {
            tasksToProcess.AddRange(tasks);
            foreach (var task in tasksToProcess)
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
                            task.isDone = true;
                            break;
                        }
                    }
                }
            }
            tasksToProcess.Clear();
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
    }
}
