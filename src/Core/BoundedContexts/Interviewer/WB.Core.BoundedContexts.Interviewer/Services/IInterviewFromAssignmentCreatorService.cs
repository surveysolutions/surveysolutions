using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewFromAssignmentCreatorService
    {
        Task CreateInterviewAsync(AssignmentDocument assignment);
    }
}