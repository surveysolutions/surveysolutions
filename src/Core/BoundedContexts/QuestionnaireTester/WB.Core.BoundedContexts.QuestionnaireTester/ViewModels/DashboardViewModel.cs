using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
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

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        private readonly IUserInteraction uiDialogs;
        private readonly IQueryablePlainStorageAccessor<QuestionnaireListItem> questionnairesStorageAccessor;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly IErrorProcessor errorProcessor;
        private readonly IRestServiceSettings restServiceSettings;
        private readonly DesignerApiServiceAccessor designerApiServiceAccessor;

        public DashboardViewModel(
            IPrincipal principal,
            ILogger logger, 
            IUserInteraction uiDialogs,
            IQueryablePlainStorageAccessor<QuestionnaireListItem> questionnairesStorageAccessor, 
            IErrorProcessor errorProcessor, 
            IRestServiceSettings restServiceSettings,
            DesignerApiServiceAccessor designerApiServiceAccessor, 
            ICommandService commandService)
            : base(logger)
        {
            this.principal = principal;
            this.uiDialogs = uiDialogs;
            this.questionnairesStorageAccessor = questionnairesStorageAccessor;
            this.errorProcessor = errorProcessor;
            this.restServiceSettings = restServiceSettings;
            this.designerApiServiceAccessor = designerApiServiceAccessor;
            this.commandService = commandService;
        }

        public async void Init()
        {
            await this.BindQuestionnairesFromStorage();
        }

        private ObservableCollection<QuestionnaireListItem> myQuestionnaires = new ObservableCollection<QuestionnaireListItem>();
        public ObservableCollection<QuestionnaireListItem> MyQuestionnaires
        {
            get { return myQuestionnaires; }
            set
            {
                myQuestionnaires = value;
                RaisePropertyChanged(() => MyQuestionnaires);
            }
        }

        private ObservableCollection<QuestionnaireListItem> publicQuestionnaires = new ObservableCollection<QuestionnaireListItem>();
        public ObservableCollection<QuestionnaireListItem> PublicQuestionnaires
        {
            get { return publicQuestionnaires; }
            set
            {
                publicQuestionnaires = value;
                RaisePropertyChanged(() => PublicQuestionnaires);
            }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set
            {
                isInProgress = value;
                RaisePropertyChanged(() => IsInProgress);
            }
        }

        private bool isPublicShowed;
        public bool IsPublicShowed
        {
            get { return isPublicShowed; }
            set
            {
                isPublicShowed = value;
                RaisePropertyChanged(() => IsPublicShowed);
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

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return signOutCommand ?? (signOutCommand = new MvxCommand(this.SignOut)); }
        }

        private IMvxCommand showSettingsCommand;
        public IMvxCommand ShowSettingsCommand
        {
            get { return showSettingsCommand ?? (showSettingsCommand = new MvxCommand(() => this.ShowViewModel<SettingsViewModel>())); }
        }

        private IMvxCommand showAboutCommand;
        public IMvxCommand ShowAboutCommand
        {
            get { return showAboutCommand ?? (showAboutCommand = new MvxCommand(() => this.ShowViewModel<AboutViewModel>())); }
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get { return loadQuestionnaireCommand ?? (loadQuestionnaireCommand = new MvxCommand<QuestionnaireListItem>(async (questionnaire)=> await this.LoadQuestionnaire(questionnaire))); }
        }

        private IMvxCommand refreshQuestionnairesCommand;
        public IMvxCommand RefreshQuestionnairesCommand
        {
            get { return refreshQuestionnairesCommand ?? (refreshQuestionnairesCommand = new MvxCommand(async () => await this.GetServerQuestionnaires(), () => !this.IsInProgress)); }
        }

        private IMvxCommand searchQuestionnairesCommand;
        public IMvxCommand SearchQuestionnairesCommand
        {
            get { return searchQuestionnairesCommand ?? (searchQuestionnairesCommand = new MvxCommand(() => this.ShowViewModel<SearchQuestionnairesViewModel>())); }
        }

        private IMvxCommand showMyQuestionnairesCommand;
        public IMvxCommand ShowMyQuestionnairesCommand
        {
            get { return showMyQuestionnairesCommand ?? (showMyQuestionnairesCommand = new MvxCommand(this.ShowMyQuestionnaires)); }
        }

        private IMvxCommand showPublicQuestionnairesCommand;
        public IMvxCommand ShowPublicQuestionnairesCommand
        {
            get { return showPublicQuestionnairesCommand ?? (showPublicQuestionnairesCommand = new MvxCommand(this.ShowPublicQuestionnaires)); }
        }

        private void ShowPublicQuestionnaires()
        {
            this.IsPublicShowed = true;
        }

        private void ShowMyQuestionnaires()
        {
            this.IsPublicShowed = false;
        }

        private async Task LoadQuestionnaire(QuestionnaireListItem selectedQuestionnaire)
        {
            this.IsServerUnavailable = false;
            this.HasErrors = false;
            this.IsInProgress = true;

            this.ProgressIndicator = UIResources.ImportQuestionnaire_CheckConnectionToServer;

            try
            {
                this.ProgressIndicator = UIResources.ImportQuestionnaire_VerifyOnServer;

                var questionnairePackage = await this.designerApiServiceAccessor.GetQuestionnaireAsync(
                    questionnaireId: selectedQuestionnaire.Id,
                    downloadProgress: (downloadProgress) =>
                    {
                        this.ProgressIndicator = string.Format(UIResources.ImportQuestionnaire_DownloadProgress, downloadProgress);
                    },
                    token: tokenSource.Token);

                this.ProgressIndicator = UIResources.ImportQuestionnaire_StoreQuestionnaire;

                this.commandService.Execute(new ImportFromDesigner(
                        createdBy: this.principal.CurrentUserIdentity.UserId,
                        source: questionnairePackage.Document,
                        allowCensusMode: true,
                        supportingAssembly: questionnairePackage.Assembly));

                this.ProgressIndicator = UIResources.ImportQuestionnaire_CreateInterview;

                var interviewId = Guid.NewGuid();

                this.commandService.Execute(new CreateInterviewOnClientCommand(
                    interviewId: interviewId,
                    userId: this.principal.CurrentUserIdentity.UserId,
                    questionnaireId: questionnairePackage.Document.PublicKey, 
                    questionnaireVersion: 1, 
                    answersTime: DateTime.UtcNow,
                    supervisorId: Guid.NewGuid()));
            
                this.ShowViewModel<PrefilledQuestionsViewModel>(new { interviewId = interviewId.FormatGuid() });
            }
            catch (RestException ex)
            {
                this.HasErrors = true;

                var error = this.errorProcessor.GetInternalErrorAndLogException(
                    ex,
                    TesterHttpAction.QuestionnaireLoading);

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
                        this.ErrorMessage = string.Format(
                            error.Message,
                            this.restServiceSettings.Endpoint.GetDomainName(),
                            selectedQuestionnaire.Id,
                            selectedQuestionnaire.Title);
                        break;

                    case ErrorCode.RequestedUrlIsForbidden:
                        this.ErrorMessage = string.Format(
                            error.Message,
                            this.restServiceSettings.Endpoint.GetDomainName(),
                            selectedQuestionnaire.Id,
                            selectedQuestionnaire.Title,
                            selectedQuestionnaire.OwnerName);
                        break;

                    default:
                        throw;
                }

                this.uiDialogs.Alert(this.ErrorMessage);
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
        }

        private Task BindQuestionnairesFromStorage()
        {
            return Task.Run(async () =>
            {
                var userQuestionnaires = this.questionnairesStorageAccessor.Query(storageModel => storageModel.OwnerName == this.principal.CurrentUserIdentity.Name);
                if (userQuestionnaires.Any())
                {
                    this.MyQuestionnaires = new ObservableCollection<QuestionnaireListItem>(userQuestionnaires.Where(qli => !qli.IsPublic));
                    this.PublicQuestionnaires = new ObservableCollection<QuestionnaireListItem>(userQuestionnaires.Where(qli => qli.IsPublic));    
                }
                else
                {
                    await this.GetServerQuestionnaires();
                }
                
            });
        }

        public async Task GetServerQuestionnaires()
        {

            this.IsInProgress = true;
            
            try
            {
                this.ClearQuestionnaires();

                int pageIndex = 1;
                foreach (var batchOfServerQuestionnaires in this.designerApiServiceAccessor.GetPagedQuestionnaires(pageIndex++, 20, tokenSource.Token))
                {
                    var convertedQuestionnaires = (await batchOfServerQuestionnaires).Select(ConvertToLocalQuestionnaireListItem);

                    this.questionnairesStorageAccessor.Store(convertedQuestionnaires.Select(qli => new Tuple<QuestionnaireListItem, string>(qli, qli.Id)));
                    this.InvokeOnMainThread(() => this.AppendToQuestionnaires(convertedQuestionnaires));
                }
            }
            catch (RestException ex)
            {
                var error = this.errorProcessor.GetInternalErrorAndLogException(ex, TesterHttpAction.FetchingQuestionnairesList);

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
                    case ErrorCode.RequestedUrlWasNotFound:
                        this.uiDialogs.Alert(error.Message);
                        break;

                    default: throw;
                }
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private void SignOut()
        {
            this.principal.SignOut();
            this.ShowViewModel<LoginViewModel>();
        }

        private QuestionnaireListItem ConvertToLocalQuestionnaireListItem(WB.Core.SharedKernels.SurveySolutions.Api.Designer.QuestionnaireListItem questionnaireListItem)
        {
            return new QuestionnaireListItem()
            {
                Id = questionnaireListItem.Id,
                Title = questionnaireListItem.Title,
                LastEntryDate = questionnaireListItem.LastEntryDate,
                IsPublic = questionnaireListItem.IsPublic,
                OwnerName = this.principal.CurrentUserIdentity.Name
            };
        }

        private void ClearQuestionnaires()
        {
            var userQuestionnaires = this.questionnairesStorageAccessor.Query(storageModel => storageModel.OwnerName == this.principal.CurrentUserIdentity.Name);
            this.questionnairesStorageAccessor.Remove(userQuestionnaires);
            this.MyQuestionnaires.Clear();
            this.PublicQuestionnaires.Clear();
        }

        private void AppendToQuestionnaires(IEnumerable<QuestionnaireListItem> questionnaires)
        {
            foreach (var questionnaireListItem in questionnaires)
            {
                if (questionnaireListItem.IsPublic)
                    this.PublicQuestionnaires.Add(questionnaireListItem);
                else
                    this.MyQuestionnaires.Add(questionnaireListItem);
            }
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}