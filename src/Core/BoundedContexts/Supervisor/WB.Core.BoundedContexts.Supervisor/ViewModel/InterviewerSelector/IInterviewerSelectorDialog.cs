using System;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector
{
    public interface IInterviewerSelectorDialog
    {
        void SelectInterviewer(AssignmentDocument assignment);
        event EventHandler Cancelled;
        event EventHandler<InterviewerSelectedArgs> Selected;
        void CloseDialog();
    }
}
