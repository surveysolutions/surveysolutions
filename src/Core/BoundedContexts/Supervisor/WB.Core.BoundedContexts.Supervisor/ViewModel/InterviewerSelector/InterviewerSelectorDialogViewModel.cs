using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector
{
    public class InterviewerSelectorDialogViewModel : MvxViewModel
    {
        private readonly IInterviewersListAccessor interviewersAccessor;

        public ICommand DoneCommand => new MvxCommand(this.Done);
        public ICommand CancelCommand => new MvxCommand(this.Cancel);

        public event EventHandler<InterviewerSelectedArgs> OnDone;
        public event EventHandler OnCancel;

        private MvxObservableCollection<InterviewerToSelectViewModel> uiItems = new MvxObservableCollection<InterviewerToSelectViewModel>();
        public MvxObservableCollection<InterviewerToSelectViewModel> UiItems
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        public IMvxAsyncCommand<InterviewerToSelectViewModel> SelectInterviewerCommand => new MvxAsyncCommand<InterviewerToSelectViewModel>(async interviewer => { await this.SelectInterviewerAsync(interviewer); });

        public InterviewerSelectorDialogViewModel(IInterviewersListAccessor interviewersAccessor)
        {
            this.interviewersAccessor = interviewersAccessor;
        }

        private async Task SelectInterviewerAsync(InterviewerToSelectViewModel interviewer)
        {
            this.uiItems.ForEach(x => x.IsSelected = false);
            this.selectedInterviewer = interviewer;
            this.selectedInterviewer.IsSelected = true;
        }

        public void Init()
        {
            this.UiItems.ReplaceWith(ToViewModels(interviewersAccessor.GetInterviewers()));
        }

        private IEnumerable<InterviewerToSelectViewModel> ToViewModels(List<InterviewerAssignInfo> interviewers)
        {
            return interviewers.Select(x => new InterviewerToSelectViewModel
            {
                Id = x.Id,
                Login = x.Login,
                FullName = x.FullaName,
                IsSelected = false
            });
        }

        private InterviewerToSelectViewModel selectedInterviewer = null;

        private string title;
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        private string note;
        public string Note
        {
            get => this.note;
            set => this.RaiseAndSetIfChanged(ref this.note, value);
        }

        private void Cancel() => this.OnCancel?.Invoke(this, EventArgs.Empty);

        private void Done()
        {
            if (selectedInterviewer == null)
            {
                this.OnCancel?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                this.OnDone?.Invoke(this, new InterviewerSelectedArgs(selectedInterviewer.Id, selectedInterviewer.Login, selectedInterviewer.FullName));
            }
        }
    }
}
