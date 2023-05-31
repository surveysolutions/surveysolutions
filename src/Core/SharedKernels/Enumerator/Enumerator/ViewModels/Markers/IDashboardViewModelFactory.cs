using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Markers;

public interface IDashboardViewModelFactory
{
    InterviewDashboardItemViewModel GetInterview(InterviewView interview);
    AssignmentDashboardItemViewModel GetAssignment(AssignmentDocument assignment);
}