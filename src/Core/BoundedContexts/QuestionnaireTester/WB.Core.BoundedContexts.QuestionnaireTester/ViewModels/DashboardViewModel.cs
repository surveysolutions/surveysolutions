using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly DesignerApiService designerApiService;

        private readonly IQuestionnaireImportService questionnaireImportService;

        public DashboardViewModel(
            IPrincipal principal,
            ILogger logger,
            DesignerApiService designerApiService, 
            ICommandService commandService, 
            IQuestionnaireImportService questionnaireImportService)
            : base(logger)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
            this.commandService = commandService;
            this.questionnaireImportService = questionnaireImportService;
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

        private bool isPublicShowed;
        public bool IsPublicShowed
        {
            get { return isPublicShowed; }
            set { isPublicShowed = value; RaisePropertyChanged(); }
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
            this.ShowViewModel<LoginViewModel>();
        }

        private void ShowPublicQuestionnaires()
        {
            this.Questionnaires = publicQuestionnaires;
        }

        private void ShowMyQuestionnaires()
        {
            this.Questionnaires = myQuestionnaires;
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

                    this.ShowViewModel<PrefilledQuestionsViewModel>(new {interviewId = interviewId.FormatGuid()});
                }
            }
            finally
            {
                this.IsInProgress = false;   
            }
        }

        public async Task GetServerQuestionnaires()
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