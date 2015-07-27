using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IUserInteractionService userInteractionService;

        readonly IPlainStorageAccessor<QuestionnaireListItem> questionnaireListStorageAccessor;

        private readonly IFriendlyMessageService friendlyMessageService;

        public DashboardViewModel(
            IPrincipal principal,
            ILogger logger,
            IDesignerApiService designerApiService, 
            ICommandService commandService, 
            IQuestionnaireImportService questionnaireImportService,
            IViewModelNavigationService viewModelNavigationService,
            IFriendlyMessageService friendlyMessageService,
            IUserInteractionService userInteractionService,
            IPlainStorageAccessor<QuestionnaireListItem> questionnaireListStorageAccessor)
            : base(logger)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
            this.commandService = commandService;
            this.questionnaireImportService = questionnaireImportService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.questionnaireListStorageAccessor = questionnaireListStorageAccessor;
            this.friendlyMessageService = friendlyMessageService;
        }

        public async void Init()
        {
            this.myQuestionnaires = this.questionnaireListStorageAccessor
                    .Query(query => query
                    .Where(questionnaire => questionnaire.OwnerName == this.principal.CurrentUserIdentity.Name)
                    .ToList());

            this.publicQuestionnaires = this.questionnaireListStorageAccessor
                    .Query(query => query
                    .Where(questionnaire => questionnaire.IsPublic)
                    .ToList());

            this.MyQuestionnairesCount = this.myQuestionnaires.Count;
            this.PublicQuestionnairesCount = this.publicQuestionnaires.Count;

            this.ShowMyQuestionnaires();

            await this.LoadServerQuestionnairesAsync();

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
            get { return loadQuestionnaireCommand ?? (loadQuestionnaireCommand = new MvxCommand<QuestionnaireListItem>(async (questionnaire) => await this.LoadQuestionnaireAsync(questionnaire))); }
        }

        private IMvxCommand refreshQuestionnairesCommand;
        public IMvxCommand RefreshQuestionnairesCommand
        {
            get { return refreshQuestionnairesCommand ?? (refreshQuestionnairesCommand = new MvxCommand(async () => await this.LoadServerQuestionnairesAsync(), () => !this.IsInProgress)); }
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
            this.IsInitialized = false;
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

        private async Task LoadQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire)
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

                    this.IsInitialized = false;
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

                if (!string.IsNullOrEmpty(errorMessage) && this.IsInitialized)
                    this.userInteractionService.Alert(errorMessage);
                else 
                    throw;
            }
            finally
            {
                this.IsInProgress = false;   
            }
        }

        private async Task LoadServerQuestionnairesAsync()
        {
            this.IsInProgress = true;

            try
            {
                await this.questionnaireListStorageAccessor.RemoveAsync(this.myQuestionnaires);
                await this.questionnaireListStorageAccessor.RemoveAsync(this.publicQuestionnaires);

                this.myQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(isPublic: false, token: tokenSource.Token);
                this.publicQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(isPublic: true, token: tokenSource.Token);

                this.MyQuestionnairesCount = this.myQuestionnaires.Count;
                this.PublicQuestionnairesCount = this.publicQuestionnaires.Count;

                this.ShowMyQuestionnaires();

                await this.questionnaireListStorageAccessor.StoreAsync(this.myQuestionnaires);
                await this.questionnaireListStorageAccessor.StoreAsync(this.publicQuestionnaires);
            }
            catch (RestException ex)
            {
                string errorMessage = this.friendlyMessageService.GetFriendlyErrorMessageByRestException(ex);

                if (!string.IsNullOrEmpty(errorMessage))
                    this.userInteractionService.Alert(errorMessage);
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