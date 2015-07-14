using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly IDesignerApiService designerApiService;

        private readonly IQuestionnaireImportService questionnaireImportService;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteraction userInteraction;
        private readonly IFriendlyMessageService friendlyMessageService;

        public DashboardViewModel(
            IPrincipal principal,
            ILogger logger,
            IDesignerApiService designerApiService, 
            ICommandService commandService, 
            IQuestionnaireImportService questionnaireImportService,
            IViewModelNavigationService viewModelNavigationService,
            IFriendlyMessageService friendlyMessageService,
            IUserInteraction userInteraction)
            : base(logger)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
            this.commandService = commandService;
            this.questionnaireImportService = questionnaireImportService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteraction = userInteraction;
            this.friendlyMessageService = friendlyMessageService;
        }

        public async void Init()
        {
            await this.GetServerQuestionnaires();
            this.IsInitialized = true;
        }

        private IList<QuestionnaireListItem> questionnaires = new QuestionnaireListItem[] { };
        public IList<QuestionnaireListItem> Questionnaires
        {
            get { return this.questionnaires; }
            set { this.questionnaires = value; RaisePropertyChanged(); }
        }

        private bool isInitialized;
        public bool IsInitialized
        {
            get { return isInitialized; }
            set { isInitialized = value; RaisePropertyChanged(); }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(); }
        }

        private string progressIndicator;
        public string ProgressIndicator
        {
            get { return progressIndicator; }
            set { progressIndicator = value; RaisePropertyChanged(); }
        }

        private int myQuestionnairesCount;
        public int MyQuestionnairesCount
        {
            get { return myQuestionnairesCount; }
            set { myQuestionnairesCount = value; RaisePropertyChanged(); }
        }

        private int publicQuestionnairesCount;
        public int PublicQuestionnairesCount
        {
            get { return publicQuestionnairesCount; }
            set { publicQuestionnairesCount = value; RaisePropertyChanged(); }
        }

        private bool isPublicShowed;
        public bool IsPublicShowed
        {
            get { return isPublicShowed; }
            set { isPublicShowed = value; RaisePropertyChanged(); }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return signOutCommand ?? (signOutCommand = new MvxCommand(this.SignOut)); }
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get { return loadQuestionnaireCommand ?? (loadQuestionnaireCommand = new MvxCommand<QuestionnaireListItem>(async (questionnaire) => await this.LoadQuestionnaire(questionnaire))); }
        }

        private IMvxCommand refreshQuestionnairesCommand;
        public IMvxCommand RefreshQuestionnairesCommand
        {
            get { return refreshQuestionnairesCommand ?? (refreshQuestionnairesCommand = new MvxCommand(async () => await this.GetServerQuestionnaires(), () => !this.IsInProgress)); }
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

        private IList<QuestionnaireListItem> myQuestionnaires;
        private IList<QuestionnaireListItem> publicQuestionnaires;

        private void SignOut()
        {
            this.principal.SignOut();
            this.viewModelNavigationService.NavigateTo<LoginViewModel>();
        }

        private void ShowPublicQuestionnaires()
        {
            this.Questionnaires = publicQuestionnaires;
            this.IsPublicShowed = true;
        }

        private void ShowMyQuestionnaires()
        {
            this.Questionnaires = myQuestionnaires;
            this.IsPublicShowed = false;
        }

        private async Task LoadQuestionnaire(QuestionnaireListItem selectedQuestionnaire)
        {
            if (this.IsInProgress) return;

            this.IsInProgress = true;

            this.ProgressIndicator = UIResources.ImportQuestionnaire_CheckConnectionToServer;

            try
            {
                var questionnairePackage = await this.designerApiService.GetQuestionnaireAsync(
                    selectedQuestionnaire: selectedQuestionnaire,
                    downloadProgress: (downloadProgress) =>
                    {
                        this.ProgressIndicator = string.Format(UIResources.ImportQuestionnaire_DownloadProgress,
                            downloadProgress);
                    },
                    token: tokenSource.Token);

                if (questionnairePackage != null)
                {
                    this.ProgressIndicator = UIResources.ImportQuestionnaire_StoreQuestionnaire;

                    questionnaireImportService.ImportQuestionnaire(questionnairePackage.Document, questionnairePackage.Assembly);
                    
                    this.ProgressIndicator = UIResources.ImportQuestionnaire_CreateInterview;

                    var interviewId = Guid.NewGuid();

                    await this.commandService.ExecuteAsync(new CreateInterviewOnClientCommand(
                        interviewId: interviewId,
                        userId: this.principal.CurrentUserIdentity.UserId,
                        questionnaireId: questionnairePackage.Document.PublicKey,
                        questionnaireVersion: 1,
                        answersTime: DateTime.UtcNow,
                        supervisorId: Guid.NewGuid()));

                    this.viewModelNavigationService.NavigateTo<PrefilledQuestionsViewModel>(new {interviewId = interviewId.FormatGuid()});
                }
            }
            catch (RestException ex)
            {
                string errorMessage;

                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Forbidden:
                        errorMessage = string.Format(UIResources.ImportQuestionnaire_Error_Forbidden, selectedQuestionnaire.Title);
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        errorMessage = String.Format(UIResources.ImportQuestionnaire_Error_PreconditionFailed, selectedQuestionnaire.Title);
                        break;
                    case HttpStatusCode.NotFound:
                        errorMessage = String.Format(UIResources.ImportQuestionnaire_Error_NotFound, selectedQuestionnaire.Title);
                        break;
                    default:
                        errorMessage = this.friendlyMessageService.GetFriendlyErrorMessageByRestException(ex);
                        break;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    this.userInteraction.Alert(errorMessage);
                else 
                    throw;
            }
            catch (OperationCanceledException)
            {
                // show here the message that loading questionnaire was canceled
                // don't needed in the current implementation
            }
            finally
            {
                this.IsInProgress = false;   
            }
        }

        private async Task GetServerQuestionnaires()
        {
            this.IsInProgress = true;

            try
            {
                this.myQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(isPublic: false, token: tokenSource.Token);
                this.publicQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(isPublic: true, token: tokenSource.Token);

                this.MyQuestionnairesCount = this.myQuestionnaires.Count;
                this.PublicQuestionnairesCount = this.publicQuestionnaires.Count;

                this.ShowMyQuestionnaires();
            }
            catch (RestException ex)
            {
                string errorMessage = this.friendlyMessageService.GetFriendlyErrorMessageByRestException(ex);

                if (!string.IsNullOrEmpty(errorMessage))
                    this.userInteraction.Alert(errorMessage);
                else
                    throw;
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}