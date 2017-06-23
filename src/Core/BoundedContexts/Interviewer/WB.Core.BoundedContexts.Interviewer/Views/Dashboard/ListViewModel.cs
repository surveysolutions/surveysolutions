using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class ListViewModel<TListItem> : InterviewTabPanel where TListItem: class
    {
        private MvxObservableCollection<IDashboardItem> uiItems;
        public MvxObservableCollection<IDashboardItem> UiItems {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }
        
        public IList<TListItem> Items;
    }
}