using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public interface IDashboardItem
    {
        bool HasExpandedView { get; }
        bool IsExpanded { get; set; }
    }

    public interface IDashboardViewItem
    {
        string Title { get; }
        string SubTitle { get; }
        string IdLabel { get; }

        MvxObservableCollection<ActionDefinition> Actions { get; }
        ActionDefinition PrimaryAction {get;}
        ActionDefinition SecondaryAction {get;}
        IEnumerable<ActionDefinition> ContextMenu { get; }
        DashboardInterviewStatus Status { get; }
    }

    public enum ActionType
    {
        Primary,
        Secondary,
        Context
    }

    public class ActionDefinition : INotifyPropertyChanged
    {
        private string label;
        public ActionType ActionType { get; set; }
        public int Tag { get; set; }

        public string Label
        {
            get => label;
            set
            {
                if (label == value) return;
                label = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled => Command?.CanExecute() ?? false;

        public IMvxCommand Command { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
