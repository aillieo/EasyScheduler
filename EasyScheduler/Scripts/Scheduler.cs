using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

namespace AillieoUtils
{
    [DefaultExecutionOrder(-100)]
    public class Scheduler : SingletonMonoBehaviour<Scheduler>
    {
        private readonly Event update = new Event();
        private readonly Event lateUpdate = new Event();
        private readonly Event fixedUpdate = new Event();
        private readonly ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();
        private readonly LinkedList<Task> managedTasks = new LinkedList<Task>();
        private readonly LinkedList<Task> managedTaskUnscaled = new LinkedList<Task>();
        
        private readonly List<Task> tasksToProcess = new List<Task>();

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

        private void Update()
        {
            ProcessTasks();

            try
            {
                update.Invoke();
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

        private void LateUpdate()
        {
            ProcessTasks();

            try
            {
                lateUpdate.Invoke();
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

        private void FixedUpdate()
        {
            try
            {
                fixedUpdate.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void Post(Action action)
        {
            Instance.tasks.Enqueue(action);
        }

        public static Task ScheduleOnce(Action action, float delay)
        {
            return CreateTask(action, 1, 0, delay, false);
        }

        public static Task ScheduleWithDelay(Action action, float interval, float delay)
        {
            return CreateTask(action, -1, interval, delay, false);
        }

        public static Task Schedule(Action action, float interval)
        {
            return CreateTask(action, -1, interval, 0, false);
        }

        public static Task ScheduleWithDelay(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, times, interval, delay, false);
        }

        public static Task Schedule(Action action, int times ,float interval)
        {
            return CreateTask(action, times, interval, 0, false);
        }

        public static Task ScheduleOnceUnscaled(Action action, float delay)
        {
            return CreateTask(action, 1, 0, delay, true);
        }

        public static Task ScheduleWithDelayUnscaled(Action action, float interval, float delay)
        {
            return CreateTask(action, -1, interval, delay, true);
        }

        public static Task ScheduleUnscaled(Action action, float interval)
        {
            return CreateTask(action, -1, interval, 0, true);
        }

        public static Task ScheduleWithDelayUnscaled(Action action, int times, float interval, float delay)
        {
            return CreateTask(action, times, interval, delay, true);
        }

        public static Task ScheduleUnscaled(Action action, int times, float interval)
        {
            return CreateTask(action, times, interval, 0, true);
        }

        private static Task CreateTask(Action action, int times, float interval, float delay, bool useUnscaledTime)
        {
            Task task = new Task() {
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

        public static bool Unschedule(Task task)
        {
            if(task == null || task.handle == null)
            {
                return false;
            }

            if(task.handle.List == Instance.managedTasks)
            {
                Instance.managedTasks.Remove(task.handle);
                task.handle = null;
                return true;
            }

            return false;
        }

        private void ProcessTasks()
        {
            Action action;
            while (tasks.Count > 0)
            {
                if (tasks.TryDequeue(out action))
                {
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
        }

        private void ProcessTimingTasks(LinkedList<Task> tasks, float delta)
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
                            break;
                        }
                    }
                }
            }
            tasksToProcess.Clear();
        }
    }
}
