using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using CAPI.Android.Core.Model.SyncCacher;
using Main.Core;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.UI.Capi.Implementations.Activities;
using WB.UI.Shared.Android.Helpers;

namespace WB.UI.Capi
{
    [Activity(Label = "Loading", NoHistory = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        private object syncLock = new object();
        private CancellationTokenSource cancellationToken;

        protected ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            cancellationToken = this.WaitForLongOperation((ct) => Restore(ct, Guid.Parse(this.Intent.GetStringExtra("publicKey"))));
        }

        public override void OnBackPressed()
        {
            if (cancellationToken != null)
                cancellationToken.Cancel();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if(cancellationToken != null)
                cancellationToken.Cancel();
        }

        protected void Restore(CancellationToken ct, Guid publicKey)
        {
            this.CheckAndRestoreFromSyncPackage(publicKey);

            var questionnaire = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(publicKey));

            if (questionnaire == null || ct.IsCancellationRequested)
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

            lock (this.syncLock)
            {
                if (!syncCacher.DoesCachedItemExist(itemKey))
                    return;

                var item = syncCacher.LoadItem(itemKey);
                if (!string.IsNullOrWhiteSpace(item))
                {
                    try
                    {
                        string content = PackageHelper.DecompressString(item);
                        var interview = JsonUtils.GetObject<InterviewSynchronizationDto>(content);

                        NcqrsEnvironment.Get<ICommandService>()
                            .Execute(new SynchronizeInterviewCommand(interview.Id, interview.UserId, interview));

                        syncCacher.DeleteItem(itemKey);
                    }
                    catch (Exception e)
                    {
                        //if state is saved as event but denormalizer failed we won't delete file
                        logger.Error("Error occured during restoring interview after synchronization", e);
                    }
                }
            }
        }


    }
}