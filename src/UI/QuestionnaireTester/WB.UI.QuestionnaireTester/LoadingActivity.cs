using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Main.Core;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using System;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.UI.QuestionnaireTester.Implementations.Activities;

namespace WB.UI.QuestionnaireTester
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
            Guid interviewId = Guid.NewGuid();
            this.LoadTemplateAndCreateInterview(publicKey, interviewId);
            
            var questionnaire = CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                    new QuestionnaireScreenInput(interviewId));

            if (questionnaire == null)
            {
                this.RunOnUiThread(this.Finish);
                return;
            }

            var intent = new Intent(this, typeof(TesterDetailsActivity));
            intent.PutExtra("publicKey", interviewId.ToString());
            this.StartActivity(intent);
        }

        private void LoadTemplateAndCreateInterview(Guid itemKey, Guid interviewId)
        {
            var token = new CancellationToken();
            var template = CapiTesterApplication.DesignerServices.GetTemplateForCurrentUser(itemKey, token);

            string content = PackageHelper.DecompressString(template.Questionnaire);
            var interview = JsonUtils.GetObject<QuestionnaireDocument>(content);

            NcqrsEnvironment.Get<ICommandService>().Execute(new ImportFromDesignerForTester(interview));
            
            Guid interviewUserId = Guid.NewGuid();

            NcqrsEnvironment.Get<ICommandService>().Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId,
                    interview.PublicKey, new Dictionary<Guid, object>(), DateTime.UtcNow));
        }
    }
}