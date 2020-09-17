using System.Threading.Tasks;

namespace WB.Services.Scheduler.Services
{
    public interface IJobsArchiver
    {
        Task<int> ArchiveJobs(string tenantName);
    }
}
