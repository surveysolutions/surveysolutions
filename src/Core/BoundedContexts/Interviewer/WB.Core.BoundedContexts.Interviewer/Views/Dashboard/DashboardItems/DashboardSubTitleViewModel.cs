using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class DashboardSubTitleViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {
        private string title;

        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        public bool HasExpandedView => false;
        
        public bool IsExpanded { get; set; }
    }
}
