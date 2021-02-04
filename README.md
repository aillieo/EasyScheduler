## Easy Scheduler

A simple but powerful Scheduler implementation in UnityEngine.

Provides methods for registering per-frame updates(Update/LateUpdate/FixedUpdate):

```c#
int updateCount = 0;
Scheduler.ScheduleUpdate(() => Debug.Log($"{++updateCount}"));

int lateUpdateCount = 0;
Scheduler.ScheduleLateUpdate(() => Debug.Log($"{++lateUpdateCount}"));

int fixedUpdateCount = 0;
Scheduler.ScheduleFixedUpdate(() => Debug.Log($"{++fixedUpdateCount}"));
```

And registering scheduling tasks for delayed or specified interval updates:

```c#

Scheduler.Delay(() => Debug.Log("Delay"));

Scheduler.ScheduleOnce(() => Debug.Log("Once"), 0.2f);

Scheduler.Schedule(() => Debug.Log("Every 0.1s and 10 times"), 10, 0.1f);

var scheduledTask = Scheduler.Schedule(() => Debug.Log("Every 0.1s"), 0.1f);
Scheduler.Unschedule(scheduledTask);

Scheduler.ScheduleWithDelay(() => Debug.Log("Every 0.1s after 2s"), 0.1f, 2);

Scheduler.ScheduleUnscaled(() => Debug.Log("Every 0.1s ignoring timescale"), 0.1f);
```

And shortcut methods for thread synchronization context:

```c#
// register in any thread and get realtimeSinceStartup in Unity main thread:

Scheduler.Send(() => Debug.Log($"{Time.realtimeSinceStartup}"));
Scheduler.Post(() => Debug.Log($"{Time.realtimeSinceStartup}"));
```

## Dependency

[UnitySingleton](https://github.com/aillieo/UnitySingleton)

Package url: `https://github.com/aillieo/UnitySingleton.git#upm`

[EasyEvent](https://github.com/aillieo/EasyEvent)

Package url: `https://github.com/aillieo/EasyEvent.git#upm`

## Usage

Clone this repository and copy it to your project folder, or add `https://github.com/aillieo/EasyScheduler.git#upm` as a dependency in the Package Manager window.
