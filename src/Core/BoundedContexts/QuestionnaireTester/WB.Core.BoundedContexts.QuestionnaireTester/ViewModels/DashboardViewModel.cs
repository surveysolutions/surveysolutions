using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        private readonly IPlainStorageAccessor<QuestionnaireListItem> questionnairesStorageAccessor;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly DesignerApiService designerApiService;

        public DashboardViewModel(
            IPrincipal principal,
            ILogger logger, IPlainStorageAccessor<QuestionnaireListItem> questionnairesStorageAccessor, 
            DesignerApiService designerApiService, 
            ICommandService commandService)
            : base(logger)
        {
            this.principal = principal;
            this.questionnairesStorageAccessor = questionnairesStorageAccessor;
            this.designerApiService = designerApiService;
            this.commandService = commandService;
        }

        public void Init()
        {
            this.BindQuestionnairesFromStorage();
        }

        private ObservableCollection<QuestionnaireListItem> myQuestionnaires = new ObservableCollection<QuestionnaireListItem>();
        public ObservableCollection<QuestionnaireListItem> MyQuestionnaires
        {
            get { return myQuestionnaires; }
            set { myQuestionnaires = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<QuestionnaireListItem> publicQuestionnaires = new ObservableCollection<QuestionnaireListItem>();
        public ObservableCollection<QuestionnaireListItem> PublicQuestionnaires
        {
            get { return publicQuestionnaires; }
            set { publicQuestionnaires = value; RaisePropertyChanged(); }
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

        private void SignOut()
        {
            this.principal.SignOut();
            this.ShowViewModel<LoginViewModel>();
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

                    this.ShowViewModel<PrefilledQuestionsViewModel>(new {interviewId = interviewId.FormatGuid()});
                }
            }
            finally
            {
                this.IsInProgress = false;   
            }
        }

        private async void BindQuestionnairesFromStorage()
        {
            var userQuestionnaires = this.questionnairesStorageAccessor.Query(
                _ => _.Where(storageModel => storageModel.OwnerName == this.principal.CurrentUserIdentity.Name).ToArray());
            if (userQuestionnaires.Any())
            {
                this.MyQuestionnaires =
                    new ObservableCollection<QuestionnaireListItem>(userQuestionnaires.Where(qli => !qli.IsPublic));
                this.PublicQuestionnaires =
                    new ObservableCollection<QuestionnaireListItem>(userQuestionnaires.Where(qli => qli.IsPublic));
            }
            else
            {
                await this.GetServerQuestionnaires();
            }
        }

        public async Task GetServerQuestionnaires()
        {
            this.IsInProgress = true;
            this.ClearQuestionnaires();

            try
            {
                await this.designerApiService.GetQuestionnairesAsync(
                    token: tokenSource.Token,
                    onPageReceived: async (batchOfServerQuestionnaires) =>
                    {
                        await this.questionnairesStorageAccessor.StoreAsync(batchOfServerQuestionnaires);
                        this.InvokeOnMainThread(() => this.AppendToQuestionnaires(batchOfServerQuestionnaires));
                    });
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private async void ClearQuestionnaires()
        {
            var userQuestionnaires = this.questionnairesStorageAccessor.Query(_ => _.Where(storageModel => storageModel.OwnerName == this.principal.CurrentUserIdentity.Name));
            await this.questionnairesStorageAccessor.RemoveAsync(userQuestionnaires);
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