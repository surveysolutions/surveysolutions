using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class MapFileCleanupTask : BaseTask
    {
        public MapFileCleanupTask(ISchedulerFactory schedulerFactory)
            : base(schedulerFactory, "Map file cleanup", typeof(MapFileCleanupJob))
        {
        }
    }
}
