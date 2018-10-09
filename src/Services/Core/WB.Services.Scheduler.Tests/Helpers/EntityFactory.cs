using System;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Tests.Helpers
{
    public class EntityFactory
    {
        public JobItem Job(string type = "test", 
            string tag = "test", 
            string tenant = "test", 
            string args = "{}",
            DateTime? scheduledAt = null)
        {
            return new JobItem
            {
                Tenant = tenant,
                Type = type,
                Tag = tag,
                Args = args,
                ScheduleAt = scheduledAt
            };
        }
    }
}
