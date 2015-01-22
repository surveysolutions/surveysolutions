using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.UI.QuestionnaireTester.Properties;
using WB.UI.QuestionnaireTester.Services;
using WB.UI.Shared.Android.Controls.ScreenItems;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;

namespace WB.UI.QuestionnaireTester.ViewModels
{
    public class QuestionnairePrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IRestService restService;
        private readonly IStringCompressor compressionUtils;
        private readonly IJsonUtils jsonUtils;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly ICommandService commandService;
        public QuestionnaireListItem Questionnaire { get; private set; }
        public Action<Guid> OnInterviewCreated { get; set; }  

        private bool isAnswering = false;
        public bool IsAnswering
        {
            get { return isAnswering; }
            set
            {
                isAnswering = value;
                RaisePropertyChanged(() => IsAnswering);
            }
        }

        private bool isInProgress = false;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set
            {
                isInProgress = value;
                RaisePropertyChanged(() => IsInProgress);
            }
        }

        public QuestionnairePrefilledQuestionsViewModel(ILogger logger, IRestService restService,
            IStringCompressor compressionUtils, IJsonUtils jsonUtils,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, ICommandService commandService,
            IPrincipal principal, IAnswerProgressIndicator answerProgressIndicator, IUserInteraction uiDialogs)
            : base(logger, principal: principal, uiDialogs: uiDialogs)
        {
            answerProgressIndicator.Setup(() => this.IsAnswering = true, () => this.IsAnswering = false);

            this.restService = restService;
            this.compressionUtils = compressionUtils;
            this.jsonUtils = jsonUtils;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.commandService = commandService;
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get
            {
                return loadQuestionnaireCommand ?? (loadQuestionnaireCommand =
                    new MvxCommand(this.LoadQuestionnaireAndCreateInterivew, () => !this.IsInProgress));
            }
        }

        private IMvxCommand openInterviewCommand;
        public IMvxCommand OpenInterviewCommand
        {
            get
            {
                return openInterviewCommand ?? (openInterviewCommand =
                    new MvxCommand(this.OpenInterview));
            }
        }

        private IMvxCommand goBackCommand;
        public IMvxCommand GoBackCommand
        {
            get
            {
                return goBackCommand ?? (goBackCommand = new MvxCommand(this.GoBack));
            }
        }

        public void Init(QuestionnaireListItem questionnaire)
        {
            this.Questionnaire = questionnaire;

            this.LoadQuestionnaireAndCreateInterivew();
        }

        private void OpenInterview()
        {
            if (this.interviewId.HasValue)
                this.ShowViewModel<InterviewViewModel>(this.interviewId);
        }

        private Guid? interviewId;

        public async void LoadQuestionnaireAndCreateInterivew()
        {
            this.IsInProgress = true;

            try
            {
                var template = await this.GetQuestionnaireFromServer();

                string content = this.compressionUtils.DecompressString(template.Questionnaire);
                var questionnaireDocument = this.jsonUtils.Deserialize<QuestionnaireDocument>(content);

                questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireDocument.PublicKey, 0,
                    template.QuestionnaireAssembly);

                this.commandService.Execute(new ImportFromDesignerForTester(questionnaireDocument));

                Guid interviewUserId = Guid.NewGuid();
                Guid interviewId = Guid.NewGuid();

                this.commandService.Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId,
                    questionnaireDocument.PublicKey, new Dictionary<Guid, object>(), DateTime.UtcNow));

                this.interviewId = interviewId;
                if (this.OnInterviewCreated != null)
                {
                    this.OnInterviewCreated(interviewId);
                }
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        this.SignOut();
                        break;
                    case HttpStatusCode.Forbidden:
                        this.UIDialogs.Alert(UIResources.ImportQuestionnaireForbidden);
                        break;
                    case HttpStatusCode.UpgradeRequired:
                        this.UIDialogs.Alert(UIResources.ImportQuestionnaireUpgradeRequired);
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        this.UIDialogs.Alert(string.Format(UIResources.ImportQuestionnairePreconditionFailed, this.Questionnaire.Title));
                        break;
                    case HttpStatusCode.NotFound:
                        this.UIDialogs.Alert(string.Format(UIResources.ImportQuestionnaireNotFound, this.Questionnaire.Title));
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        this.UIDialogs.Alert(UIResources.ImportQuestionnaireServiceUnavailable);
                        break;
                    case HttpStatusCode.RequestTimeout:
                        this.UIDialogs.Alert(UIResources.ImportQuestionnaireRequestTimeout);
                        break;
                    default:
                        throw;
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error("Exception when downloading questionnaire/creating interview", ex);
                this.UIDialogs.Alert(ex.Message);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public async Task<QuestionnaireCommunicationPackage> GetQuestionnaireFromServer()
        {
            var supportedVersion = QuestionnaireVersionProvider.GetCurrentEngineVersion();

            return await this.restService.PostAsync<QuestionnaireCommunicationPackage>(
                url: "questionnaire",
                credentials:
                    new RestCredentials()
                    {
                        Login = this.Principal.CurrentIdentity.Name,
                        Password = this.Principal.CurrentIdentity.Password
                    },
                request: new DownloadQuestionnaireRequest()
                {
                    QuestionnaireId = Questionnaire.Id,
                    SupportedVersion = new QuestionnaireVersion()
                    {
                        Major = supportedVersion.Major,
                        Minor = supportedVersion.Minor,
                        Patch = supportedVersion.Patch
                    }
                });
        }
    }
}