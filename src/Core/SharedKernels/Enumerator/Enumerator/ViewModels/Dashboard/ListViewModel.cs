using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class ListViewModel : InterviewTabPanel
    {
        public bool IsItemsLoaded { get; protected set; }
        public event EventHandler OnItemsLoaded;
        protected abstract IEnumerable<IDashboardItem> GetUiItems();

        private MvxObservableCollection<IDashboardItem> uiItems = new MvxObservableCollection<IDashboardItem>();
        public MvxObservableCollection<IDashboardItem> UiItems {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged( ref this.uiItems, value);
        }

        private int itemsCount;
        public int ItemsCount
        {
            get => this.itemsCount;
            protected set => this.RaiseAndSetIfChanged( ref this.itemsCount, value);
        }

        public async Task UpdateUiItemsAsync() 
        {
            this.IsItemsLoaded = false;

            try
            {
                await this.InvokeOnMainThreadAsync(() =>
                {
                    var newItems = this.GetUiItems();
                    
                    this.UiItems.ToList().ForEach(uiItem =>
                    {
                        if (uiItem is IDashboardItemWithEvents withEvents)
                            withEvents.OnItemUpdated -= ListViewModel_OnItemUpdated;
                        uiItem.DisposeIfDisposable();
                    });
                    
                    this.UiItems.ReplaceWith(newItems);

                    this.UiItems.ToList().ForEach(item =>
                    {
                        if (item is IDashboardItemWithEvents withEvents)
                            withEvents.OnItemUpdated += ListViewModel_OnItemUpdated;
                    });
                }, false).ConfigureAwait(false);
                
            }
            finally
            {
                this.IsItemsLoaded = true;
            }
            
            this.OnItemsLoaded?.Invoke(this, EventArgs.Empty);
        }

        protected void ListViewModel_OnItemUpdated(object sender, EventArgs args)
        {
            if (sender is IDashboardItemWithEvents dashboardItem)
            {
                var newDashboardItem = GetUpdatedDashboardItem(dashboardItem);
                
                var indexOf = UiItems.IndexOf(dashboardItem);
                UiItems[indexOf] = newDashboardItem;

                dashboardItem.OnItemUpdated -= ListViewModel_OnItemUpdated;
                newDashboardItem.OnItemUpdated += ListViewModel_OnItemUpdated;
            }
        }

        protected virtual IDashboardItemWithEvents GetUpdatedDashboardItem(IDashboardItemWithEvents dashboardItem)
        {
            throw new ArgumentException("Need implement this method to refresh dashboard item");
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            this.UiItems.ToList()
                    .Select(i => i as IDashboardItemWithEvents)
                    .ForEach(i => i?.RefreshDataTime());
        }
    }
}
