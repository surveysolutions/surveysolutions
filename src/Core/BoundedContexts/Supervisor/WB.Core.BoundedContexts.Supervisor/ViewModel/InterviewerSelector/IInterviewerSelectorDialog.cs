using System;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector
{
    public interface IInterviewerSelectorDialog
    {
        void SelectInterviewer(string title);
        event EventHandler Cancelled;
        event EventHandler<InterviewerSelectedArgs> Selected;
        void CloseDialog();
    }
}
