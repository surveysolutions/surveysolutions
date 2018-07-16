using System;
using MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector
{
    public class InterviewerToSelectViewModel : MvxViewModel
    {
        private Guid id;
        private string login;
        private string fullName;
        private bool isSelected;

        public Guid Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref this.id, value);
        }

        public string Login
        {
            get => login;
            set => this.RaiseAndSetIfChanged(ref this.login, value);
        }

        public string FullName
        {
            get => fullName;
            set => this.RaiseAndSetIfChanged(ref this.fullName, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }
    }
}
