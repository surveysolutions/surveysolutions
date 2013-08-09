using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ModelUtils;
using CAPI.Android.Core.Model.SyncCacher;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    [Activity(Label = "Loading", NoHistory = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        private Action<Guid> restore; 
        protected override void OnCreate(Bundle bundle)
        {
            restore = Restore;
            base.OnCreate(bundle);
            ProgressBar pb=new ProgressBar(this);
            
           /* TextView tv=new TextView(this);
            var img = this.Resources.GetDrawable(Android.Resource.Drawable.SpinnerDropDownBackground);
            //img.SetBounds(0, 0, 45, 45);
            tv.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);*/
            this.AddContentView(pb, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
            restore.BeginInvoke(Guid.Parse(Intent.GetStringExtra("publicKey")), Callback, restore);
            // Create your application here
        }
        private void Callback(IAsyncResult asyncResult)
        {
            Action<Guid> asyncAction = (Action<Guid>)asyncResult.AsyncState;
            asyncAction.EndInvoke(asyncResult);
        }
        protected void Restore(Guid publicKey)
        {
            try
            {
                var documentStorage = CapiApplication.Kernel.Get<IReadSideRepositoryWriter<CompleteQuestionnaireView>>();
                var result = documentStorage.GetById(publicKey);
                if (result == null)
                {
                    GenerateEvents(publicKey);
                }
                Intent intent = new Intent(this, typeof(DetailsActivity));
                intent.PutExtra("publicKey", publicKey.ToString());
                StartActivity(intent);
            }
            catch (Exception exc)
            {
                var logger = ServiceLocator.Current.GetInstance<ILogger>();
                logger.Error("Rebuild Error", exc);
            }
        }
#warning remove after eluminating ncqrs
        private void GenerateEvents(Guid publicKey)
        {
            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            var snapshotStore = NcqrsEnvironment.Get<ISnapshotStore>();

            //loading from sync cache 
            var syncCacher = CapiApplication.Kernel.Get<ISyncCacher>();
            var item = syncCacher.LoadItem(publicKey);
            if (!string.IsNullOrWhiteSpace(item))
            {
                string content = PackageHelper.DecompressString(item);
                var questionnarieContent = JsonUtils.GetObject<CompleteQuestionnaireDocument>(content);
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new CreateNewAssigment(questionnarieContent));

                syncCacher.DeleteItem(publicKey);
            }


            var documentStorage = CapiApplication.Kernel.Get<IReadSideRepositoryWriter<CompleteQuestionnaireView>>();
            long minVersion = 0;
            var snapshot = snapshotStore.GetSnapshot(publicKey, long.MaxValue);
            if (snapshot != null)
            {
                var originalDoc = snapshot.Payload as CompleteQuestionnaireDocument;
                if (originalDoc != null)
                {
                    documentStorage.Store(
                        new CompleteQuestionnaireView(originalDoc),
                        publicKey);
                    minVersion = snapshot.Version + 1;
                }
            }
            var eventsAfterSnapshot = eventStore.ReadFrom(publicKey, minVersion, long.MaxValue);
            foreach (CommittedEvent committedEvent in eventsAfterSnapshot)
            {
                try
                {
                    bus.Publish(committedEvent);
                }
                catch(Exception e)
                {
                    var logger = ServiceLocator.Current.GetInstance<ILogger>();

                    logger.Error("Rebuild Error", e);

                    logger.Error("Event: " + JsonUtils.GetJsonData(committedEvent.Payload));
                }
            }
        }
    }
}