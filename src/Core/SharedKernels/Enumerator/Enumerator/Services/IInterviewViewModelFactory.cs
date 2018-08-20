using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IInterviewViewModelFactory
    {
        List<IInterviewEntityViewModel> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState);
        IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestions(string interviewId);
        T GetNew<T>() where T : class;
        IDashboardItem GetDashboardAssignment(AssignmentDocument assignment);
        IDashboardItem GetDashboardInterview(InterviewView interviewView, List<PrefilledQuestion> details);
    }
}
