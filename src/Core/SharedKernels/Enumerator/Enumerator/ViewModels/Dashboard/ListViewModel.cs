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
                var newItems = await Task.Run(this.GetUiItems).ConfigureAwait(false);

                this.UiItems.ToList().ForEach(uiItem =>
                {
                    uiItem.OnItemUpdated -= ListViewModel_OnItemUpdated;
                    uiItem.DisposeIfDisposable();
                });

                await this.InvokeOnMainThreadAsync(() =>
                {
                    this.UiItems.ReplaceWith(newItems);

                }, false).ConfigureAwait(false);
                this.UiItems.ForEach(item => item.OnItemUpdated += ListViewModel_OnItemUpdated);
            }
            finally
            {
                this.IsItemsLoaded = true;
            }
            
            this.OnItemsLoaded?.Invoke(this, EventArgs.Empty);
        }
        
        protected virtual void ListViewModel_OnItemUpdated(object sender, EventArgs args) { }
    }
}
