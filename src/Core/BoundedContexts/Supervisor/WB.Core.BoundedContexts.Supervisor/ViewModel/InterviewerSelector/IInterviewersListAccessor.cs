using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector
{
    public interface IInterviewersListAccessor
    {
        List<InterviewerAssignInfo> GetInterviewers();
    }
}