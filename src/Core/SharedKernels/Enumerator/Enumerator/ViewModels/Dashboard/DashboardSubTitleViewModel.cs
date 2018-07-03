using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public class DashboardSubTitleViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {
        private string title;

        public string Title
        {
            get => this.title;
            set => MvxNotifyPropertyChangedExtensions.RaiseAndSetIfChanged(this, ref this.title, value);
        }

        public bool HasExpandedView => false;
        
        public bool IsExpanded { get; set; }
    }
}
