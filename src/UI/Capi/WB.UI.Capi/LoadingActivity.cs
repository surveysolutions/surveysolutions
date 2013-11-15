using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using CAPI.Android.Core.Model.SyncCacher;
using Main.Core;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.UI.Capi.Implementations.Activities;

namespace WB.UI.Capi
{
    [Activity(Label = "Loading", NoHistory = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        private Action<Guid> restore; 

        protected override void OnCreate(Bundle bundle)
        {
            this.restore = this.Restore;
            base.OnCreate(bundle);
            var pb = new ProgressBar(this);
            this.AddContentView(pb, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            this.restore.BeginInvoke(Guid.Parse(this.Intent.GetStringExtra("publicKey")), this.Callback, this.restore);
        }
        private void Callback(IAsyncResult asyncResult)
        {
            var asyncAction = (Action<Guid>)asyncResult.AsyncState;
            asyncAction.EndInvoke(asyncResult);
        }

        protected void Restore(Guid publicKey)
        {
            this.CheckAndRestoreFromSyncPackage(publicKey);
            
            var questionnaire = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(publicKey));
            if (questionnaire == null)
            {
                this.RunOnUiThread(this.Finish);
                return;
            }
            var intent = new Intent(this, typeof(DataCollectionDetailsActivity));
            intent.PutExtra("publicKey", publicKey.ToString());
            this.StartActivity(intent);
        }

        private void CheckAndRestoreFromSyncPackage(Guid itemKey)
        {
            var syncCacher = CapiApplication.Kernel.Get<ISyncCacher>();

            if (!syncCacher.DoesCachedItemExist(itemKey))
                return;

            var item = syncCacher.LoadItem(itemKey);
            if (!string.IsNullOrWhiteSpace(item))
            {
                string content = PackageHelper.DecompressString(item);
                var interview = JsonUtils.GetObject<InterviewSynchronizationDto>(content);

                NcqrsEnvironment.Get<ICommandService>().Execute(new SynchronizeInterviewCommand(interview.Id, interview.UserId, interview));
                
            }
            syncCacher.DeleteItem(itemKey);


        }
    }
}