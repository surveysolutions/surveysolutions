using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class ListViewModel : InterviewTabPanel
    {
        public bool IsItemsLoaded { get; protected set; }
        public event EventHandler OnItemsLoaded;
        protected abstract IEnumerable<IDashboardItem> GetUiItems();

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

        protected void UpdateUiItems() => Task.Run(() =>
        {
            this.IsItemsLoaded = false;

            try
            {
                var newItems = this.GetUiItems().ToList();
                this.UiItems.ReplaceWith(newItems);
            }
            finally
            {
                this.IsItemsLoaded = true;
            }
            
            this.OnItemsLoaded?.Invoke(this, EventArgs.Empty);
        });
    }
}
