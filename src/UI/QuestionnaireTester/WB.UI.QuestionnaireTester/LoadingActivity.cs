using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Main.Core;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using System;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.UI.Shared.Android.Extensions;
using WB.UI.Shared.Android.Helpers;

namespace WB.UI.QuestionnaireTester
{
    [Activity(NoHistory = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        protected ILogger logger;
        private CancellationTokenSource longOperationTokenSource;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            this.longOperationTokenSource =
                this.WaitForLongOperation((ct) => Restore(ct, Guid.Parse(this.Intent.GetStringExtra("publicKey"))));
        }

        public override void OnBackPressed()
        {
            this.RunOnUiThread(this.Finish);
            base.OnBackPressed();
        }

        public override void Finish()
        {
            this.longOperationTokenSource.Cancel();
            base.Finish();
        }

        protected void Restore(CancellationToken ct, Guid publicKey)
        {
            Guid interviewId = Guid.NewGuid();

            if (!LoadTemplateAndCreateInterview(publicKey, interviewId, ct))
                this.RunOnUiThread(() => CapiTesterApplication.Context.ClearAllBackStack<QuestionnaireListActivity>());
            else
            {
                var questionnaire = CapiTesterApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                    new QuestionnaireScreenInput(interviewId));

                if (questionnaire == null || ct.IsCancellationRequested)
                    return;

                var intent = new Intent(this, typeof (CreateInterviewActivity));
                intent.PutExtra("publicKey", interviewId.ToString());

                if (!ct.IsCancellationRequested)
                    this.StartActivity(intent);
            }
        }

        private bool LoadTemplateAndCreateInterview(Guid itemKey, Guid interviewId, CancellationToken ct)
        {
            if (!CapiTesterApplication.DesignerMembership.IsLoggedIn)
                return false;

            QuestionnaireCommunicationPackage template;
            try
            {
                template = CapiTesterApplication.DesignerServices.GetTemplateForCurrentUser(CapiTesterApplication.DesignerMembership.RemoteUser, itemKey, ct);
            }
            catch (Exception exc) 
            {
                ShowLongToastInUIThread(exc.Message);
                return false;
            }

            if (ct.IsCancellationRequested) 
                return false;

            if (template == null)
            {
                ShowLongToastInUIThread("Template is missing.");
                return false;
            }

            try
            {
                string content = PackageHelper.DecompressString(template.Questionnaire);
                var questionnaireDocument = JsonUtils.GetObject<QuestionnaireDocument>(content);

                var assemblyFileAccessor = ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyFileAccessor>();
                assemblyFileAccessor.StoreAssembly(questionnaireDocument.PublicKey, 0, template.QuestionnaireAssembly);

                NcqrsEnvironment.Get<ICommandService>().Execute(new ImportFromDesignerForTester(questionnaireDocument));

                Guid interviewUserId = Guid.NewGuid();

                NcqrsEnvironment.Get<ICommandService>().Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId,
                    questionnaireDocument.PublicKey, new Dictionary<Guid, object>(), DateTime.UtcNow));
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                ShowLongToastInUIThread("Template is not valid for current version of Tester.");
                
                return false;
            }

            return true;
        }

        private void ShowLongToastInUIThread(string message)
        {
            this.RunOnUiThread(() => Toast.MakeText(this, message, ToastLength.Long).Show());
        }
    }
}