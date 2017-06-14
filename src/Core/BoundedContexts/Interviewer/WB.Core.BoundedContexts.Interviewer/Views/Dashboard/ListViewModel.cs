using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class ListViewModel<TListItem> : InterviewTabPanel where TListItem: class
    {
        public List<IDashboardItem> UiItems { get; protected set; }

        private IList<TListItem> items;
        public IList<TListItem> Items
        {
            get { return this.items; }
            set { this.items = value; this.RaisePropertyChanged(); }
        }
    }
}