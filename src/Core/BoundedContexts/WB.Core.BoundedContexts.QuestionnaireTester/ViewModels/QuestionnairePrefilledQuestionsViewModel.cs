using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class QuestionnairePrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IRestService restService;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly ICommandService commandService;
        private readonly IPlainStorageAccessor<QuestionnaireDocument> questionnairesStorage;
        public QuestionnaireMetaInfo Questionnaire { get; private set; }
        public Action<Guid> OnInterviewCreated { get; set; }
        public Action<Guid> OnInterviewDetailsOpened { get; set; }

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

        private bool canCreateInterview = false;
        public bool CanCreateInterview
        {
            get { return canCreateInterview; }
            set
            {
                canCreateInterview = value;
                RaisePropertyChanged(() => CanCreateInterview);
            }
        }

        private string progressIndicator;
        public string ProgressIndicator
        {
            get { return progressIndicator; }
            set
            {
                progressIndicator = value;
                RaisePropertyChanged(() => ProgressIndicator);
            }
        }

        public QuestionnairePrefilledQuestionsViewModel(ILogger logger, IRestService restService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, ICommandService commandService,
            IPrincipal principal, IAnswerProgressIndicator answerProgressIndicator, IUserInteraction uiDialogs,
            IPlainStorageAccessor<QuestionnaireDocument> questionnairesStorage)
            : base(logger, principal: principal, uiDialogs: uiDialogs)
        {
            answerProgressIndicator.Setup(() => this.IsAnswering = true, () => this.IsAnswering = false);

            this.restService = restService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.commandService = commandService;
            this.questionnairesStorage = questionnairesStorage;
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get
            {
                return loadQuestionnaireCommand ?? (loadQuestionnaireCommand =
                    new MvxCommand(async ()=>await this.LoadQuestionnaireAndCreateInterivew(), () => !this.IsInProgress));
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

        public async void Init(QuestionnaireMetaInfo questionnaire)
        {
            this.Questionnaire = questionnaire;

            await this.LoadQuestionnaireAndCreateInterivew();
        }

        private void OpenInterview()
        {
            if (this.interviewId.HasValue && this.OnInterviewDetailsOpened != null)
                this.OnInterviewDetailsOpened(this.interviewId.Value);

        }

        private Guid? interviewId;

        public Task LoadQuestionnaireAndCreateInterivew()
        {
            return Task.Run(async () =>
            {
                this.IsInProgress = true;

                try
                {
                    this.ProgressIndicator = UIResources.ImportQuestionnaire_VerifyOnServer;

                    var questionnaireDocumentFromStorage = this.questionnairesStorage.GetById(this.Questionnaire.Id);

                    if (questionnaireDocumentFromStorage == null ||
                        questionnaireDocumentFromStorage.LastEntryDate != (await this.GetQuestionnaireLastEntryDate()).LastEntryDate)
                    {
                        var questionnaireCommunicationPackage = await this.GetQuestionnaireFromServer(
                            (downloadProgress) => { this.ProgressIndicator = string.Format(UIResources.ImportQuestionnaire_DownloadProgress, downloadProgress); });

                        this.ProgressIndicator = UIResources.ImportQuestionnaire_StoreQuestionnaire;

                        this.questionnairesStorage.Store(questionnaireCommunicationPackage.Questionnaire, questionnaireCommunicationPackage.Questionnaire.PublicKey.FormatGuid());
                        questionnaireDocumentFromStorage = questionnaireCommunicationPackage.Questionnaire;

                        this.ProgressIndicator = UIResources.ImportQuestionnaire_StoreAssembly;

                        this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireCommunicationPackage.Questionnaire.PublicKey, 0,
                            questionnaireCommunicationPackage.QuestionnaireAssembly);
                    }

                    this.ProgressIndicator = UIResources.ImportQuestionnaire_PrepareQuestionnaire;

                    this.ExecuteImportFromDesignerForTesterCommand(questionnaireDocumentFromStorage);

                    Guid interviewUserId = Guid.NewGuid();
                    Guid interviewId = Guid.NewGuid();

                    this.ProgressIndicator = UIResources.ImportQuestionnaire_CreateInterview;

                    this.ExecuteCreateInterviewCommand(interviewId, interviewUserId, questionnaireDocumentFromStorage.PublicKey);

                    this.interviewId = interviewId;
                    if (this.OnInterviewCreated != null)
                    {
                        this.InvokeOnMainThread(() => this.OnInterviewCreated(interviewId));
                    }
                    this.CanCreateInterview = true;
                }
                catch (RestException ex)
                {
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            this.SignOut();
                            break;
                        case HttpStatusCode.Forbidden:
                            this.UIDialogs.Alert(UIResources.ImportQuestionnaire_Error_Forbidden);
                            break;
                        case HttpStatusCode.UpgradeRequired:
                            this.UIDialogs.Alert(UIResources.ImportQuestionnaire_Error_UpgradeRequired);
                            break;
                        case HttpStatusCode.PreconditionFailed:
                            this.UIDialogs.Alert(string.Format(UIResources.ImportQuestionnaire_Error_PreconditionFailed, this.Questionnaire.Title));
                            break;
                        case HttpStatusCode.NotFound:
                            this.UIDialogs.Alert(string.Format(UIResources.ImportQuestionnaire_Error_NotFound, this.Questionnaire.Title));
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            this.UIDialogs.Alert(ex.Message.Contains("maintenance")
                                ? UIResources.Maintenance
                                : UIResources.ServiceUnavailable);
                            break;
                        case HttpStatusCode.RequestTimeout:
                            this.UIDialogs.Alert(UIResources.RequestTimeout);
                            break;
                        default:
                            throw;
                    }
                }
                finally
                {
                    this.IsInProgress = false;
                }
            });
        }

        private void ExecuteCreateInterviewCommand(Guid interviewId, Guid interviewUserId, Guid questionnaireId)
        {
            this.commandService.Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId, questionnaireId, new Dictionary<Guid, object>(), DateTime.UtcNow));
        }

        private void ExecuteImportFromDesignerForTesterCommand(QuestionnaireDocument template)
        {
            this.commandService.Execute(new ImportFromDesignerForTester(template));
        }

        private async Task<QuestionnaireMetaInfo> GetQuestionnaireLastEntryDate()
        {
            return await this.restService.GetAsync<QuestionnaireMetaInfo>(
                url: string.Format("questionnaires/{0}/meta", Questionnaire.Id),
                credentials:
                    new RestCredentials()
                    {
                        Login = this.Principal.CurrentIdentity.Name,
                        Password = this.Principal.CurrentIdentity.Password
                    }
                );
        }

        public async Task<QuestionnaireResponse> GetQuestionnaireFromServer(Action<decimal> downloadProgress)
        {
            return await this.restService.GetWithProgressAsync<QuestionnaireResponse>(
                url: string.Format("questionnaires/{0}", Questionnaire.Id),
                credentials:
                    new RestCredentials()
                    {
                        Login = this.Principal.CurrentIdentity.Name,
                        Password = this.Principal.CurrentIdentity.Password
                    },
                progressPercentage: downloadProgress, token: new CancellationToken());
        }

        public override void GoBack()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}