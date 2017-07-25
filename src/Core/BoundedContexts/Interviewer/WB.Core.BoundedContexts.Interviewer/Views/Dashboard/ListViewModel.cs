using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class ListViewModel : InterviewTabPanel
    {
        private MvxObservableCollection<IDashboardItem> uiItems = new MvxObservableCollection<IDashboardItem>();
        public MvxObservableCollection<IDashboardItem> UiItems {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        private int itemsCount;
        public int ItemsCount
        {
            get => this.itemsCount;
            protected set => this.RaiseAndSetIfChanged(ref this.itemsCount, value);
        }
    }
}