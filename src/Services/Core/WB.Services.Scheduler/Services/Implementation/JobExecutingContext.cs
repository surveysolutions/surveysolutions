using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Services.Implementation
{
    public class JobExecutingContext
    {
        public JobExecutingContext(JobItem job)
        {
            Job = job;
        }

        public JobItem Job { get; set; }
    }
}