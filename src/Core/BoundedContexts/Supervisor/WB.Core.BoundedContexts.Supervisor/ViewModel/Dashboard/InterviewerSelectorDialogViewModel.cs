using System;
using System.Windows.Input;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public class InterviewerSelectorDialogViewModel : MvxViewModel
    {
        public ICommand DoneCommand => new MvxCommand(this.Done);
        public ICommand CancelCommand => new MvxCommand(this.Cancel);

        public event EventHandler OnDone;
        public event EventHandler OnCancel;

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
