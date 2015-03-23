using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.UI.Shared.Android.Helpers;
using Environment = System.Environment;

namespace WB.UI.QuestionnaireTester
{
    [Activity(NoHistory = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        private CancellationTokenSource longOperationTokenSource;
        private string additionalMassage = string.Empty;

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private IArchiveUtils Archive
        {
            get { return ServiceLocator.Current.GetInstance<IArchiveUtils>(); }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.ActionBar.SetDisplayShowHomeEnabled(false);
            longOperationTokenSource = this.WaitForLongOperation((ct) => this.Restore(ct, Guid.Parse(this.Intent.GetStringExtra("publicKey")), this.Intent.GetStringExtra("questionnaireTitle")));
        }

        public override void OnBackPressed()
        {
            this.RunOnUiThread(this.Finish);
            base.OnBackPressed();
        }

        public override void Finish()
        {
            if (longOperationTokenSource != null && longOperationTokenSource.Token.CanBeCanceled)
            {
                this.longOperationTokenSource.Cancel();
            }

            base.Finish();
        }

        protected void ShowErrorMassageToUser()
        {
            this.longOperationTokenSource = null;
            this.ActionBar.SetDisplayShowHomeEnabled(true);

            var container = new LinearLayout(this);
            container.Orientation = Orientation.Vertical;
            container.SetGravity(GravityFlags.CenterVertical);

            var messageView = new TextView(this);
            messageView.SetPadding(10, 30, 10, 30);
            messageView.SetBackgroundResource(Resource.Drawable.errorwarningstyle);
            var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.RightMargin = layoutParams.LeftMargin = 15;
            messageView.LayoutParameters = layoutParams;
            messageView.SetTextColor(Color.Black);
            messageView.SetTextSize(ComplexUnitType.Dip, 20);
            messageView.Text = Resources.GetText(Resource.String.Oops) + 
                (string.IsNullOrEmpty(additionalMassage)
                ? string.Empty
                : Environment.NewLine + string.Format(Resources.GetText(Resource.String.ReasonFormat), additionalMassage));
            container.AddView(messageView);

            this.AddContentView(container, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));

            this.Title = Resources.GetText(Resource.String.QuestionnaireLoadingError);
        }

        protected async Task Restore(CancellationToken ct, Guid publicKey, string questionnaireTitle)
        {
            Guid interviewId = Guid.NewGuid();

            var loaded = await LoadTemplateAndCreateInterview(publicKey, questionnaireTitle, interviewId, ct);
            if (ct.IsCancellationRequested)
                return;
            
            if (!loaded)
                this.RunOnUiThread(ShowErrorMassageToUser);
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

        private async Task<bool> LoadTemplateAndCreateInterview(Guid itemKey, string questionnaireTitle, Guid interviewId, CancellationToken ct)
        {
            if (!CapiTesterApplication.DesignerMembership.IsLoggedIn)
                return false;

            QuestionnaireCommunicationPackage template;
            try
            {
                template = await CapiTesterApplication.DesignerServices.GetTemplateForCurrentUser(CapiTesterApplication.DesignerMembership.RemoteUser, itemKey, ct);
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.UpgradeRequired:
                        additionalMassage = ex.Message;
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        additionalMassage = string.Format(ErrorMessages.Questionnaire_verification_failed, questionnaireTitle);
                        break;
                    case HttpStatusCode.NotFound:
                        additionalMassage = string.Format(ErrorMessages.TemplateNotFound, questionnaireTitle);
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        additionalMassage = ErrorMessages.ServiceUnavailable;
                        break;
                    case HttpStatusCode.RequestTimeout:
                        additionalMassage = ErrorMessages.RequestTimeout;
                        break;
                    default:
                        additionalMassage = ErrorMessages.ServerError;
                        break;
                }
                return false;
            }
            catch (Exception exc)
            {
                Logger.Error(exc.Message,exc);
                additionalMassage = exc.Message;
                return false;
            }

            if (ct.IsCancellationRequested) 
                return false;

            if (template == null)
            {
                additionalMassage = Resources.GetText(Resource.String.TemplateIsMissing);
                return false;
            }

            try 
            {
                string content = Archive.DecompressString(template.Questionnaire);
                var questionnaireDocument = ServiceLocator.Current.GetInstance<IJsonUtils>().Deserialize<QuestionnaireDocument>(content);

                var assemblyFileAccessor = ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyFileAccessor>();
                assemblyFileAccessor.StoreAssembly(questionnaireDocument.PublicKey, 0, template.QuestionnaireAssembly);

                ServiceLocator.Current.GetInstance<ICommandService>().Execute(new ImportFromDesignerForTester(questionnaireDocument));

                Guid interviewUserId = Guid.NewGuid();

                ServiceLocator.Current.GetInstance<ICommandService>().Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId,
                    questionnaireDocument.PublicKey, new Dictionary<Guid, object>(), DateTime.UtcNow));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                additionalMassage = Resources.GetText(Resource.String.TemplateIsNotValidForCurrentVersionOfTester);
                additionalMassage += Environment.NewLine + e.Message;
                return false;
            }

            return true;
        }
    }
}