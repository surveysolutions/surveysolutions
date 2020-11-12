using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IInterviewerVersionReader
    {
        Task<int?> InterviewerBuildNumber();
        Task<int?> SupervisorBuildNumber();
    }
}
