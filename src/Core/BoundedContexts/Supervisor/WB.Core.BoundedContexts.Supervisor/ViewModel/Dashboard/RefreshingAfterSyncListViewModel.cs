using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Commands;
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
            messengerSubscription = Messenger.Subscribe<DashboardChangedMsg>(async msg => await this.UpdateUiItemsAsync(), MvxReference.Strong);
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();
            messengerSubscription.Dispose();
        }
    }
}
