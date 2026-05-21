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

        protected override IDashboardItemWithEvents GetUpdatedDashboardItem(IDashboardItemWithEvents dashboardItem)
        {
            // After a status change the item may need to move to a different group, so trigger a
            // full list refresh. Return the existing item temporarily; it will be replaced when
            // UpdateUiItemsAsync completes and rebuilds the list.
            _ = UpdateUiItemsAsync();
            return dashboardItem;
        }
    }
}
