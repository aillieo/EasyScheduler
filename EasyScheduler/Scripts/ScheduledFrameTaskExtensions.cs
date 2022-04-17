namespace AillieoUtils
{
    public static class ScheduledFrameTaskExtensions
    {
        public static bool Unschedule(this ScheduledFrameTask scheduledFrameTask)
        {
            return Scheduler.Unschedule(scheduledFrameTask);
        }
    }
}
