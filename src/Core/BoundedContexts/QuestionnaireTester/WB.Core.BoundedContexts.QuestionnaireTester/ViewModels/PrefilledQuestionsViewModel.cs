using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IRestService restService;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IUserInteraction uiDialogs;
        private readonly IPlainStorageAccessor<QuestionnaireListItem> questionnaireInfoAccessor;
        private readonly IPlainStorageAccessor<Questionnaire> questionnairesStorage;
        private readonly IRestServiceSettings restServiceSettings;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly IErrorProcessor errorProcessor;


        private Guid newInterviewId;
        private QuestionnaireListItem selectedQuestionnaire;

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

        private bool hasErrors = false;
        public bool HasErrors
        {
            get { return hasErrors; }
            set
            {
                hasErrors = value;
                RaisePropertyChanged(() => HasErrors);
            }
        }

        private bool isServerUnavailable = false;
        public bool IsServerUnavailable
        {
            get { return isServerUnavailable; }
            set
            {
                isServerUnavailable = value;
                RaisePropertyChanged(() => IsServerUnavailable);
            }
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                RaisePropertyChanged(() => ErrorMessage);
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

        public PrefilledQuestionsViewModel(ILogger logger, IRestService restService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, ICommandService commandService,
            IPrincipal principal, IUserInteraction uiDialogs, IPlainStorageAccessor<QuestionnaireListItem> questionnaireInfoAccessor,
            IPlainStorageAccessor<Questionnaire> questionnairesStorage, IRestServiceSettings restServiceSettings, IErrorProcessor errorProcessor)
            : base(logger)
        {
            this.restService = restService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.commandService = commandService;
            this.principal = principal;
            this.uiDialogs = uiDialogs;
            this.questionnaireInfoAccessor = questionnaireInfoAccessor;
            this.questionnairesStorage = questionnairesStorage;
            this.restServiceSettings = restServiceSettings;
            this.errorProcessor = errorProcessor;
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get
            {
                return loadQuestionnaireCommand ?? (loadQuestionnaireCommand =
                    new MvxCommand(async () => await this.LoadQuestionnaireAndCreateInterview(), () => !this.IsInProgress));
            }
        }

        private IMvxCommand tryAgainToLoadQuestionnaireCommand;
        public IMvxCommand TryAgainToLoadQuestionnaireCommand
        {
            get
            {
                return tryAgainToLoadQuestionnaireCommand ?? (tryAgainToLoadQuestionnaireCommand =
                    new MvxCommand(async () => await this.LoadQuestionnaireAndCreateInterview(), () => !this.IsInProgress));
            }
        }

        private IMvxCommand openInterviewCommand;
        public IMvxCommand OpenInterviewCommand
        {
            get
            {
                return openInterviewCommand ?? (openInterviewCommand = new MvxCommand(this.OpenInterview, () => this.CanCreateInterview));
            }
        }

        public async void Init(string questionnaireId)
        {
            this.selectedQuestionnaire = this.questionnaireInfoAccessor.GetById(questionnaireId);

            await this.LoadQuestionnaireAndCreateInterview();
        }

        private void OpenInterview()
        {
            this.ShowViewModel<InterviewGroupViewModel>(this.newInterviewId);
        }

        public Task LoadQuestionnaireAndCreateInterview()
        {
            return Task.Run(async () =>
            {
                this.IsServerUnavailable = false;
                this.HasErrors = false;
                this.IsInProgress = true;

                this.ProgressIndicator = UIResources.ImportQuestionnaire_CheckConnectionToServer;

                var questionnaireDocumentFromStorage = this.questionnairesStorage.GetById(this.selectedQuestionnaire.Id);

                try
                {
                    //var questionnaireMetaInfo = await this.GetQuestionnaireMetaInfo();

                    if (questionnaireDocumentFromStorage == null /*|| questionnaireDocumentFromStorage.Questionnaire.LastEntryDate != questionnaireMetaInfo.LastEntryDate*/)
                    {
                        this.ProgressIndicator = UIResources.ImportQuestionnaire_VerifyOnServer;

                        var questionnaireCommunicationPackage = await this.GetQuestionnaireFromServer(
                            (downloadProgress) =>
                            {
                                this.ProgressIndicator = string.Format(UIResources.ImportQuestionnaire_DownloadProgress, downloadProgress);
                            });

                        this.ProgressIndicator = UIResources.ImportQuestionnaire_StoreQuestionnaire;

                        this.questionnairesStorage.Store(questionnaireCommunicationPackage, questionnaireCommunicationPackage.Id);

                        this.ProgressIndicator = UIResources.ImportQuestionnaire_StoreAssembly;

                        this.questionnaireAssemblyFileAccessor.StoreAssembly(Guid.Parse(questionnaireCommunicationPackage.Id), 0, questionnaireCommunicationPackage.Assembly);

                        await this.CreateInterview(questionnaireDocumentFromStorage.Document);
                    }
                }
                catch (RestException ex)
                {
                    this.HasErrors = true;

                    var error = this.errorProcessor.GetInternalErrorAndLogException(ex, TesterHttpAction.QuestionnaireLoading);

                    switch (error.Code)
                    {
                        case ErrorCode.UserWasNotAuthoredOnDesigner:
                        case ErrorCode.AccountIsLockedOnDesigner:
                            this.SignOut();
                            break;

                        case ErrorCode.DesignerIsInMaintenanceMode:
                        case ErrorCode.DesignerIsUnavailable:
                        case ErrorCode.RequestTimeout:
                        case ErrorCode.InternalServerError:
                            this.ErrorMessage = error.Message;
                            this.IsServerUnavailable = true;
                            break;
                            
                        case ErrorCode.TesterUpgradeIsRequiredToOpenQuestionnaire:
                            this.ErrorMessage = error.Message;
                            break;

                        case ErrorCode.QuestionnaireContainsErrorsAndCannotBeOpened:
                        case ErrorCode.RequestedUrlWasNotFound:
                            this.ErrorMessage = string.Format(error.Message,
                                this.restServiceSettings.Endpoint.GetDomainName(), 
                                this.selectedQuestionnaire.Id,
                                this.selectedQuestionnaire.Title);
                            break;

                        case ErrorCode.RequestedUrlIsForbidden:
                            this.ErrorMessage = string.Format(error.Message,
                                this.restServiceSettings.Endpoint.GetDomainName(), 
                                this.selectedQuestionnaire.Id,
                                this.selectedQuestionnaire.Title, 
                                this.selectedQuestionnaire.OwnerName);
                            break;
                       
                        default: throw;
                    }
                   
                    if (questionnaireDocumentFromStorage != null && this.IsServerUnavailable)
                    {
                        if (this.tokenSource.IsCancellationRequested) return;
                        this.uiDialogs.Confirm(UIResources.ImportQuestionnaire_OpenLocalInterview, async () =>
                        {
                            await this.CreateInterview(questionnaireDocumentFromStorage.Document);
                        }, UIResources.ConfirmationText, UIResources.ConfirmationYesText, UIResources.ConfirmationNoText);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // show here the message that loading questionnaire was canceled
                    // don't needed in the current implementation
                }
                finally
                {
                    this.IsInProgress = false;
                }

            }, tokenSource.Token);
        }

        private void SignOut()
        {
            this.principal.SignOut();
            this.ShowViewModel<LoginViewModel>();
        }

        private Task CreateInterview(QuestionnaireDocument questionnaireDocument)
        {
            return Task.Run(() =>
            {
                this.IsServerUnavailable = false;
                this.HasErrors = false;
                this.IsInProgress = true;

                this.ProgressIndicator = UIResources.ImportQuestionnaire_PrepareQuestionnaire;

                if (tokenSource.IsCancellationRequested) return;
                this.ExecuteImportFromDesignerForTesterCommand(questionnaireDocument);

                this.ProgressIndicator = UIResources.ImportQuestionnaire_CreateInterview;

                Guid interviewUserId = Guid.NewGuid();
                this.newInterviewId = Guid.NewGuid();

                if (tokenSource.IsCancellationRequested) return;
                this.ExecuteCreateInterviewCommand(this.newInterviewId, interviewUserId, questionnaireDocument.PublicKey);

                if (tokenSource.IsCancellationRequested) return;
                this.InvokeOnMainThread(this.OpenInterview);

                this.CanCreateInterview = true;
                this.IsInProgress = false;

            }, this.tokenSource.Token);
        }

        private void ExecuteCreateInterviewCommand(Guid interviewId, Guid interviewUserId, Guid questionnaireId)
        {
            this.commandService.Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId, questionnaireId, new Dictionary<Guid, object>(), DateTime.UtcNow));
        }

        private void ExecuteImportFromDesignerForTesterCommand(QuestionnaireDocument template)
        {
            this.commandService.Execute(new ImportFromDesignerForTester(template));
        }

        //private async Task<QuestionnaireMetaInfo> GetQuestionnaireMetaInfo()
        //{
        //    return await this.restService.GetAsync<QuestionnaireMetaInfo>(
        //        url: string.Format("questionnaires/{0}/meta", selectedQuestionnaire.Id),
        //        credentials:
        //            new RestCredentials()
        //            {
        //                Login = this.principal.CurrentUserIdentity.Name,
        //                Password = this.principal.CurrentUserIdentity.Password
        //            },
        //        token: tokenSource.Token);
        //}

        public async Task<Questionnaire> GetQuestionnaireFromServer(Action<decimal> downloadProgress)
        {
            return await this.restService.GetWithProgressAsync<Questionnaire>(
                url: string.Format("questionnaires/{0}", selectedQuestionnaire.Id),
                credentials:
                    new RestCredentials()
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                progressPercentage: downloadProgress, token: tokenSource.Token);
        }

        public override void NavigateToPreviousViewModel()
        {
            this.tokenSource.Cancel();
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}