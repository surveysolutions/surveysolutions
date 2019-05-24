using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public abstract class RefreshingAfterSyncListViewModel : ListViewModel
    {
        private readonly IMvxMessenger messenger;

        protected RefreshingAfterSyncListViewModel(IMvxMessenger messenger)
        {
            this.messenger = messenger;
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
            messengerSubscription = messenger.Subscribe<DashboardChangedMsg>(async msg => await this.UpdateUiItemsAsync());
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();
            messengerSubscription.Dispose();
        }
    }
}
