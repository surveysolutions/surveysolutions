using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MvvmCross.Core.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
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
        ActionDefinition[] ContextMenu { get; }
        DashboardInterviewStatus Status { get; }

        bool IsAssignment { get; }
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
        private DashboardInterviewStatus colorStatus;
       
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

        public DashboardInterviewStatus ColorStatus
        {
            get => colorStatus;
            set
            {
                if (colorStatus == value) return;
                colorStatus = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}