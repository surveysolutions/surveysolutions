using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services
{
    public interface IInterviewerSelectorDialog
    {
        void SelectInterviewer(AssignmentDocument assignment);
    }
}
