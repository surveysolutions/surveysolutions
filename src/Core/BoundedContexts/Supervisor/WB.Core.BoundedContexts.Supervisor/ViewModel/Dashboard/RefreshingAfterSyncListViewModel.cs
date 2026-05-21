using System;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public abstract class RefreshingAfterSyncListViewModel : ListViewModel
    {
        protected readonly IMvxMessenger Messenger;

        protected RefreshingAfterSyncListViewModel()
        {
            Messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
        }

        private MvxSubscriptionToken messengerSubscription;

        public override async Task Initialize()
        {
            await base.Initialize();
            await UpdateUiItemsAsync();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            messengerSubscription = Messenger.Subscribe<DashboardChangedMessage>(async msg => await this.UpdateUiItemsAsync(), MvxReference.Strong);
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();
            messengerSubscription?.Dispose();
        }

        protected override void ListViewModel_OnItemUpdated(object sender, EventArgs args)
        {
            if (sender is IDashboardItemWithEvents dashboardItem)
            {
                // Refresh the item in-place first so buttons update immediately.
                dashboardItem.Refresh();

                // Then schedule a full list rebuild so items are correctly grouped /
                // removed after the status change.  Calling UpdateUiItemsAsync() here
                // (rather than inside GetUpdatedDashboardItem) is safe because this
                // override does NOT touch UiItems[indexOf] afterwards, eliminating the
                // ArgumentOutOfRangeException race that occurred with the old approach.
                _ = UpdateUiItemsAsync();
            }
        }
    }
}
