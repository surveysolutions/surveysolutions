using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    public class InterviewEntitiesListFragment : BaseFragment<ActiveStageViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_active_group;


        private MvxSubscriptionToken scrollToAnchorSubscriptionToken;

        private MvxRecyclerView recyclerView;
        private LinearLayoutManager layoutManager;
        private InterviewEntityAdapter adapter;


        /*protected override void OnCreate(Bundle savedInstanceState)
        {
            this.recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.interviewEntitiesList);

            this.layoutManager = new LinearLayoutManager(this);
            this.recyclerView.SetLayoutManager(this.layoutManager);
            this.recyclerView.HasFixedSize = true;

            this.adapter = new InterviewEntityAdapter((IMvxAndroidBindingContext)this.BindingContext);
            this.recyclerView.Adapter = this.adapter;

            base.OnCreate(savedInstanceState);
        }

        protected override void OnStart()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            this.scrollToAnchorSubscriptionToken = messenger.Subscribe<ScrollToAnchorMessage>(this.OnScrollToAnchorMessage);
            base.OnStart();
        }

        protected override void OnStop()
        {
            var messenger = Mvx.Resolve<IMvxMessenger>();
            messenger.Unsubscribe<ScrollToAnchorMessage>(this.scrollToAnchorSubscriptionToken);
            base.OnStop();
        }

        private void OnScrollToAnchorMessage(ScrollToAnchorMessage msg)
        {
            if (this.layoutManager != null)
            {
                Application.SynchronizationContext.Post(_ =>
                {
                    this.layoutManager.ScrollToPositionWithOffset(msg.AnchorElementIndex, 0);
                },
                null);
            }
        }*/
    }
}