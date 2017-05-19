using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class ListViewModel<TListItem> : TabPanel where TListItem: class
    {
        private IList<TListItem> items;
        public IList<TListItem> Items
        {
            get { return this.items; }
            set { this.items = value; this.RaisePropertyChanged(); }
        }
    }
}