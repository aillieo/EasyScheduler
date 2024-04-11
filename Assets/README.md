## Easy Scheduler

Easy Scheduler is a powerful and simple scheduler implementation for Unity. It provides various methods for registering callbacks to Unity player loop update events, scheduling tasks, executing actions on the Unity main thread, and managing Unity coroutines.

### Update Event Registration

Easy Scheduler allows you to register and unregister callbacks to Unity player loop update events, including Update, LateUpdate, and FixedUpdate.

```c#
// Register a callback to the Update event
int updateCount = 0;
Scheduler.ScheduleUpdate(() => Debug.Log($"{++updateCount}"));

// Register a callback to the LateUpdate event
int lateUpdateCount = 0;
Scheduler.ScheduleLateUpdate(() => Debug.Log($"{++lateUpdateCount}"));

// Register a callback to the FixedUpdate event
int fixedUpdateCount = 0;
Scheduler.ScheduleFixedUpdate(() => Debug.Log($"{++fixedUpdateCount}"));
```

These methods enable you to execute specific actions during each frame update event.

### Task Scheduling

#### Schedule a Task by a Fixed Time Interval

```c#
// Execute a task after a specified delay
Scheduler.ScheduleOnce(() => Debug.Log("Once"), 0.2f);

// Execute a task every 0.1 seconds for 10 times
Scheduler.Schedule(() => Debug.Log("Every 0.1s and 10 times"), 10, 0.1f);

// Schedule a task to execute every 0.1 seconds
var scheduledTask = Scheduler.Schedule(() => Debug.Log("Every 0.1s"), 0.1f);
// Unschedule the previously scheduled task
Scheduler.Unschedule(scheduledTask);

// Schedule a task to execute every 0.1 seconds after a delay of 2 seconds
Scheduler.ScheduleWithDelay(() => Debug.Log("Every 0.1s after 2s"), 0.1f, 2);

// Schedule a task to execute every 0.1 seconds ignoring the timescale
Scheduler.ScheduleUnscaled(() => Debug.Log("Every 0.1s ignoring timescale"), 0.1f);

// Execute a task as soon as possible after the current call stack
Scheduler.Delay(() => Debug.Log("Delay"));
```

The above code demonstrates different ways to schedule tasks by a fixed time interval. You can use the `ScheduleOnce` method to execute a task after a specified delay. The `Schedule` method allows you to execute a task repeatedly for a specified number of times at regular intervals. The `ScheduleWithDelay` method schedules a task to execute repeatedly at regular intervals after a specified delay. The `ScheduleUnscaled` method schedules a task to execute repeatedly at regular intervals without considering the timescale.

#### Schedule a Task by a Fixed Frame Interval

```c#
// Schedule a task to execute on the next frame
Scheduler.ScheduleNextFrame(() => Debug.Log("Next frame"));

// Schedule a task to execute after 10 frames
Scheduler.ScheduleAfterFrames(() => Debug.Log("After 10 frames"), 10);

// Schedule a task to execute every 10 frames
Scheduler.ScheduleByFrame(() => Debug.Log("Every 10 frames"), 10);
```

The above code demonstrates how to schedule tasks by a fixed frame interval. The `ScheduleNextFrame` method schedules a task to execute on the next frame. The `ScheduleAfterFrames` method schedules a task to execute after a specified number of frames. The `ScheduleByFrame` method schedules a task to execute repeatedly at regular intervals specified by the frame count.

### Thread Synchronization Context

Easy Scheduler provides methods for sending or posting callbacks to the Unity main thread, allowing you to execute actions in a synchronized manner.

```c#
// Register a callback and execute it on the Unity main thread
Scheduler.Send(() => Debug.Log("Sent to Unity main thread"));

// Register a callback and post it to the Unity main thread
Scheduler.Post(() => Debug.Log("Posted to Unity main thread"));
```

These methods are useful when you need to perform actions that require access to Unity-specific APIs on the main thread while registering them from other threads.

### Coroutine Management

Easy Scheduler provides functions for starting and stopping Unity coroutines.

```c#
// Start a Unity coroutine
Coroutine coroutine = Scheduler.StartCoroutine(MyCoroutine());

// Stop a Unity coroutine
Scheduler.StopCoroutine(coroutine);
```

These methods allow you to manage the execution of Unity coroutines within your project.

## Dependencies

Easy Scheduler has the following dependencies:

- [UnitySingleton](https://github.com/aillieo/UnitySingleton): A Unity package for implementing the Singleton pattern in Unity projects. You can find the package at the following URL: `https://github.com/aillieo/UnitySingleton.git#upm`

- [EasyEvent](https://github.com/aillieo/EasyEvent): A package for implementing event systems in Unity projects. You can find the package at the following URL: `https://github.com/aillieo/EasyEvent.git#upm`

Make sure to install these dependencies before using Easy Scheduler in your project.

## Installation

To install Easy Scheduler, you have two options:

1. Clone this repository and copy it to your project folder.

2. Add `https://github.com/aillieo/EasyScheduler.git#upm` as a dependency in the Package Manager window.

Choose the installation method that suits your project requirements.