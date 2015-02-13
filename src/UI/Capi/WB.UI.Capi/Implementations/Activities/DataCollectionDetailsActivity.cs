using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Implementations.Adapters;
using WB.UI.Capi.SnapshotStore;
using WB.UI.Shared.Android.Activities;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Controls.ScreenItems;

namespace WB.UI.Capi.Implementations.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class DataCollectionDetailsActivity : DetailsActivity
    {
        private IAnswerProgressIndicator AnswerProgressIndicator
        {
            get { return ServiceLocator.Current.GetInstance<IAnswerProgressIndicator>(); }
        }

        protected override ContentFrameAdapter CreateFrameAdapter(InterviewItemId? screenId)
        {
            return new DataCollectionContentFrameAdapter(this.SupportFragmentManager, this.ViewModel as InterviewViewModel, screenId);
        }

        protected override InterviewViewModel GetInterviewViewModel(Guid interviewId)
        {
            return CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(new QuestionnaireScreenInput(interviewId));
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetupActionBar();
        }

        public override void Finish()
        {
            base.Finish();

            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>() as FileBasedSnapshotStore;
            if (snapshotStore != null)
                snapshotStore.PersistShapshot(this.QuestionnaireId);
        }

        private void SetupActionBar()
        {
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            this.ActionBar.SetDisplayShowTitleEnabled(false);
            this.ActionBar.SetDisplayShowCustomEnabled(true);
            this.ActionBar.SetDisplayUseLogoEnabled(true);
            this.ActionBar.SetCustomView(Resource.Layout.InterviewActionBar);
            
            var txtTitle = (TextView)this.ActionBar.CustomView.FindViewById(Resource.Id.txtTitle);
            txtTitle.Text = Title;

            var imgProgress = (ImageView)this.ActionBar.CustomView.FindViewById(Resource.Id.imgAnswerProgress);

            this.AnswerProgressIndicator.Setup(
                show: () => this.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Visible),
                hide: () => this.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Invisible));
        }
    }
}