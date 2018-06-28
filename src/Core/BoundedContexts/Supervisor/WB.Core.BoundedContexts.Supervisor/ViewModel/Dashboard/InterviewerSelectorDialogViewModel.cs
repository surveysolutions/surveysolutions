using System;
using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class InterviewerSelectorDialogViewModel : MvxViewModel
    {
        private readonly IInterviewersListAccessor interviewersAccessor;

        public ICommand DoneCommand => new MvxCommand(this.Done);
        public ICommand CancelCommand => new MvxCommand(this.Cancel);

        public event EventHandler OnDone;
        public event EventHandler OnCancel;

        private MvxObservableCollection<InterviewerAssignInfo> uiItems = new MvxObservableCollection<InterviewerAssignInfo>();
        public MvxObservableCollection<InterviewerAssignInfo> UiItems
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        public InterviewerSelectorDialogViewModel(IInterviewersListAccessor interviewersAccessor)
        {
            this.interviewersAccessor = interviewersAccessor;
        }

        public void Init()
        {
            this.UiItems.ReplaceWith(interviewersAccessor.GetInterviewers());
        }

        private string title;
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        private void Cancel() => this.OnCancel?.Invoke(this, EventArgs.Empty);

        private void Done() => this.OnDone?.Invoke(this, EventArgs.Empty);
    }
}
