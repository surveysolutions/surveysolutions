using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewFromAssignmentCreatorService
    {
        Task CreateInterviewAsync(int assignmentId);
    }
}