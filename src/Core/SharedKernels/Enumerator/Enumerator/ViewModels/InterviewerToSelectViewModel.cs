using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class InterviewerToSelectViewModel : MvxNotifyPropertyChanged
    {
        private readonly SelectResponsibleForAssignmentViewModel parent;
        public InterviewerToSelectViewModel(SelectResponsibleForAssignmentViewModel parent)
        {
            this.parent = parent;
        }

        public Guid Id { get; set; }

        public string Login { get; set; }

        public string FullName { get; set; }
        public bool IsEnabled { get; set; }
        public int InterviewsCount { get; set; }

        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }

        public IMvxCommand SelectCommand => new MvxCommand(Select);

        private void Select() => this.parent.SelectInterviewerCommand.Execute(this);
    }
}
