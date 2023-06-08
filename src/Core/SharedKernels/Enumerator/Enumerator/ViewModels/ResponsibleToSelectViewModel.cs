using System;
using Main.Core.Entities.SubEntities;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class ResponsibleToSelectViewModel : MvxNotifyPropertyChanged
    {
        private readonly Action<ResponsibleToSelectViewModel> selectedAction;
        public ResponsibleToSelectViewModel(Action<ResponsibleToSelectViewModel> selectedAction)
        {
            this.selectedAction = selectedAction;
        }

        public Guid Id { get; set; }

        public string Login { get; set; }

        public string FullName { get; set; }
        public int InterviewsCount { get; set; }

        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }

        public IMvxCommand SelectCommand => new MvxCommand(Select);
        public UserRoles Role { get; set; }

        private void Select() => this.selectedAction?.Invoke(this);
    }
}
