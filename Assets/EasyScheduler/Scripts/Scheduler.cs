using System.Text;

namespace AillieoUtils
{
    public static partial class Scheduler
    {
        public static bool HasInstance => SchedulerImpl.HasInstance;

        public static float GlobalTimeScale
        {
            get => SchedulerImpl.Instance.globalTimeScale;
            set => SchedulerImpl.Instance.globalTimeScale = value;
        }

        public static int UpdatePhase
        {
            get => SchedulerImpl.Instance.updatePhase;
        }

        public static string GetRunningInfo()
        {
            if (!SchedulerImpl.HasInstance)
            {
                return string.Empty;
            }

            SchedulerImpl ins = SchedulerImpl.Instance;
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Scheduler:");
            stringBuilder.AppendLine($"  GlobalTimeScale : {ins.globalTimeScale}");
            stringBuilder.AppendLine($"  UpdatePhase : {ins.updatePhase}");

            stringBuilder.AppendLine("Running Tasks:");
            stringBuilder.AppendLine($"  Dynamic : {ins.managedDynamicTasks.Count} + {ins.managedDynamicTasksUnscaled.Count}");
            stringBuilder.AppendLine($"  Static : {ins.managedStaticTasks.Count} + {ins.managedStaticTasksUnscaled.Count}");
            stringBuilder.AppendLine($"  Long term : {ins.managedLongTermTasks.Count} + {ins.managedLongTermTasksUnscaled.Count}");

            stringBuilder.AppendLine("Running Frame Tasks:");
            stringBuilder.AppendLine($"  Dynamic : {ins.managedDynamicFrameTasks.Count}");
            stringBuilder.AppendLine($"  Static : {ins.managedStaticFrameTasks.Count}");

            stringBuilder.AppendLine("Running Updates:");
            stringBuilder.AppendLine($"  PreUpdate : {ins.preUpdate.ListenerCount}");
            stringBuilder.AppendLine($"  FixedUpdate : {ins.fixedUpdate.ListenerCount}");
            stringBuilder.AppendLine($"  Update : {ins.update.ListenerCount}");
            stringBuilder.AppendLine($"  LateUpdate : {ins.lateUpdate.ListenerCount}");

            return stringBuilder.ToString();
        }
    }
}
