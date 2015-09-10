using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Cirrious.MvvmCross.ViewModels;
using Humanizer;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Services.Infrastructure;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Entities;
using WB.Core.SharedKernels.Enumerator.ViewModels;

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
        private readonly List<QuestionnaireListItem> questionnaireListStorageCache = new List<QuestionnaireListItem>();
        private readonly IAsyncPlainStorage<QuestionnaireListItem> questionnaireListStorage;
        private readonly IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage;

        private readonly IFriendlyErrorMessageService friendlyErrorMessageService;

        public DashboardViewModel(
            IPrincipal principal,
            IDesignerApiService designerApiService, 
            ICommandService commandService, 
            IQuestionnaireImportService questionnaireImportService,
            IViewModelNavigationService viewModelNavigationService,
            IFriendlyErrorMessageService friendlyErrorMessageService,
            IUserInteractionService userInteractionService,
            IAsyncPlainStorage<QuestionnaireListItem> questionnaireListStorage, 
            IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
            this.commandService = commandService;
            this.questionnaireImportService = questionnaireImportService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.questionnaireListStorage = questionnaireListStorage;
            this.dashboardLastUpdateStorage = dashboardLastUpdateStorage;
            this.friendlyErrorMessageService = friendlyErrorMessageService;
        }

        public async void Init()
        {
            questionnaireListStorageCache.AddRange(this.questionnaireListStorage.Query(query => query
                .Where(questionnaire => questionnaire.OwnerName == this.principal.CurrentUserIdentity.Name || questionnaire.IsPublic)
                .ToList()));

            if (!questionnaireListStorageCache.Any())
            {
                await LoadServerQuestionnairesAsync();
            }
            else
            {
                this.LoadFilteredListFromLocalStorage(null);
            }

            this.ShowEmptyQuestionnaireListText = true;
            this.IsSearchVisible = false;

            var lastUpdate = this.dashboardLastUpdateStorage.Query(query => query.FirstOrDefault(x => x.Id == this.principal.CurrentUserIdentity.Name));

            this.HumanizeLasUpdateDate(lastUpdate == null ? (DateTime?)null : lastUpdate.LastUpdateDate);
        }
       
        private void LoadFilteredListFromLocalStorage(string searchTerm)
        {
            var trimmedSearchText = (searchTerm ?? "").Trim();

            Func<QuestionnaireListItem, bool> epmtyFilter = x => true;
            Func<QuestionnaireListItem, bool> titleSearchFilter = x => x.Title.Contains(trimmedSearchText);
            Func<QuestionnaireListItem, bool> searchFilter = string.IsNullOrEmpty(trimmedSearchText)
                ? epmtyFilter
                : titleSearchFilter;

            var myQuestionnaireListItems = questionnaireListStorageCache
                .Where(questionnaire => questionnaire.OwnerName == this.principal.CurrentUserIdentity.Name)
                .Where(searchFilter)
                .ToList();

            var publicQuestionnaireListItems = questionnaireListStorageCache
                .Where(questionnaire => questionnaire.IsPublic)
                .Where(searchFilter)
                .ToList();

            this.myQuestionnaires = this.HightlightSearchTermInFilteredList(
                myQuestionnaireListItems,
                trimmedSearchText);
            this.publicQuestionnaires = this.HightlightSearchTermInFilteredList(
                publicQuestionnaireListItems,
                trimmedSearchText);

            this.MyQuestionnairesCount = this.myQuestionnaires.Count;
            this.PublicQuestionnairesCount = this.publicQuestionnaires.Count;

            if (!IsPublicShowed) this.ShowMyQuestionnaires();
            else this.ShowPublicQuestionnaires();
        }

        private string humanizedLastUpdateDate;
        public string HumanizedLastUpdateDate
        {
            get { return this.humanizedLastUpdateDate; }
            set { this.humanizedLastUpdateDate = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<QuestionnaireListItem> questionnaires = new ObservableCollection<QuestionnaireListItem>();
        public ObservableCollection<QuestionnaireListItem> Questionnaires
        {
            get { return this.questionnaires; }
            set { 
                this.questionnaires = value; 
                RaisePropertyChanged(); 
            }
        }

        private bool showEmptyQuestionnaireListText;
        public bool ShowEmptyQuestionnaireListText
        {
            get { return this.showEmptyQuestionnaireListText; }
            set { this.showEmptyQuestionnaireListText = value; RaisePropertyChanged(); }
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

        private bool isSearchVisible;
        public bool IsSearchVisible
        {
            get { return isSearchVisible; }
            set { isSearchVisible = value; RaisePropertyChanged(); }
        }

        private IMvxCommand clearSearchCommand;
        public IMvxCommand ClearSearchCommand
        {
            get { return clearSearchCommand ?? (clearSearchCommand = new MvxCommand(this.ClearSearch)); }
        }

        public IMvxCommand ShowSearchCommand
        {
            get { return new MvxCommand(this.ShowSearch, () => !this.IsInProgress); }
        }

        private void ShowSearch()
        {
            IsSearchVisible = true;
        }

        private void ClearSearch()
        {
            this.SearchText = null;
            IsSearchVisible = false;
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return signOutCommand ?? (signOutCommand = new MvxCommand(this.SignOut)); }
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get { return loadQuestionnaireCommand ?? 
                (loadQuestionnaireCommand = new MvxCommand<QuestionnaireListItem>(async (questionnaire) => await this.LoadQuestionnaireAsync(questionnaire), 
               (item) => !this.IsInProgress)); }
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

        private string searchText;
        public string SearchText
        {
            get { return this.searchText; }
            set {  
                this.searchText = value;
                RaisePropertyChanged(); 
                LoadFilteredListFromLocalStorage(searchText);
            }
        }

        private List<QuestionnaireListItem> myQuestionnaires;
        private List<QuestionnaireListItem> publicQuestionnaires;

        private void SignOut()
        {
            this.principal.SignOut();
            this.viewModelNavigationService.NavigateTo<LoginViewModel>();
        }

        private void ShowPublicQuestionnaires()
        {
            this.Questionnaires.Clear();
            publicQuestionnaires.ForEach(x => this.Questionnaires.Add(x));
            RaisePropertyChanged(() => Questionnaires);
            this.IsPublicShowed = true;
        }

        private void ShowMyQuestionnaires()
        {
            this.Questionnaires.Clear();
            myQuestionnaires.ForEach(x => this.Questionnaires.Add(x));
            RaisePropertyChanged(() => Questionnaires);
            this.IsPublicShowed = false;
        }

        private async Task LoadQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire)
        {
            if (this.IsInProgress) return;

            this.IsInProgress = true;

            this.ProgressIndicator = UIResources.ImportQuestionnaire_CheckConnectionToServer;

            string errorMessage = null;
            try
            {
                var questionnairePackage = await this.designerApiService.GetQuestionnaireAsync(
                    selectedQuestionnaire: selectedQuestionnaire,
                    downloadProgress: (downloadProgress) =>
                    {
                        this.ProgressIndicator = string.Format(UIResources.ImportQuestionnaire_DownloadProgress,
                            downloadProgress);
                    },
                    token: this.tokenSource.Token);

                if (questionnairePackage != null)
                {
                    this.ProgressIndicator = UIResources.ImportQuestionnaire_StoreQuestionnaire;

                    var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111-1111-1111-1111-111111111111"), 1);
                    var questionnaireDocument = questionnairePackage.Document;
                    var supportingAssembly = questionnairePackage.Assembly;

                    questionnaireDocument.PublicKey = questionnaireIdentity.QuestionnaireId;

                    this.questionnaireImportService.ImportQuestionnaire(questionnaireIdentity, questionnaireDocument, supportingAssembly);

                    this.ProgressIndicator = UIResources.ImportQuestionnaire_CreateInterview;

                    var interviewId = Guid.NewGuid();

                    await this.commandService.ExecuteAsync(new CreateInterviewOnClientCommand(
                        interviewId: interviewId,
                        userId: this.principal.CurrentUserIdentity.UserId,
                        questionnaireId: questionnaireIdentity.QuestionnaireId,
                        questionnaireVersion: questionnaireIdentity.Version,
                        answersTime: DateTime.UtcNow,
                        supervisorId: Guid.NewGuid()));

                    this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
                }
            }
            catch (RestException ex)
            {
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
                        errorMessage = this.friendlyErrorMessageService.GetFriendlyErrorMessageByRestException(ex);
                        break;
                }

                if (string.IsNullOrEmpty(errorMessage))
                    throw;
            }
            finally
            {
                this.IsInProgress = false;   
            }

            if (!string.IsNullOrEmpty(errorMessage))
                await this.userInteractionService.AlertAsync(errorMessage);
        }

        private async Task LoadServerQuestionnairesAsync()
        {
            this.IsInProgress = true;
            string errorMessage = null;
            try
            {
                ClearSearch();

                await this.questionnaireListStorage.RemoveAsync(this.myQuestionnaires);
                await this.questionnaireListStorage.RemoveAsync(this.publicQuestionnaires);
                questionnaireListStorageCache.Clear();

                var loadedMyQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(isPublic: false, token: tokenSource.Token);
                var loadedPublicQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(isPublic: true, token: tokenSource.Token);

                questionnaireListStorageCache.AddRange(loadedMyQuestionnaires);
                questionnaireListStorageCache.AddRange(loadedPublicQuestionnaires);

                await this.questionnaireListStorage.StoreAsync(questionnaireListStorageCache);

                var lastUpdateDate = DateTime.Now.ToLocalTime();

                HumanizeLasUpdateDate(lastUpdateDate);

                await this.dashboardLastUpdateStorage.StoreAsync(new DashboardLastUpdate
                {
                    Id = this.principal.CurrentUserIdentity.Name,
                    LastUpdateDate = lastUpdateDate
                });

                LoadFilteredListFromLocalStorage(null);
            }
            catch (RestException ex)
            {
                errorMessage = this.friendlyErrorMessageService.GetFriendlyErrorMessageByRestException(ex);

                if (string.IsNullOrEmpty(errorMessage))
                    throw;
            }
            finally
            {
                this.IsInProgress = false;
            }

            if (!string.IsNullOrEmpty(errorMessage))
                await this.userInteractionService.AlertAsync(errorMessage);
        }

        private void HumanizeLasUpdateDate(DateTime? lastUpdate)
        {
            this.HumanizedLastUpdateDate = lastUpdate.HasValue 
                ? string.Format(UIResources.Dashboard_LastUpdated, lastUpdate.Value.Humanize(utcDate:false))
                : UIResources.Dashboard_HasNotBeenUpdated;
        }

        private List<QuestionnaireListItem> HightlightSearchTermInFilteredList(List<QuestionnaireListItem> questionnaireListItems, string searchTerm)
        {
            if (searchTerm.IsNullOrEmpty() || !questionnaireListItems.Any())
            {
                return questionnaireListItems;
            }

            return questionnaireListItems.Select(x => HightlightTitleInListItem(x, searchTerm)).ToList();
        }

        private static QuestionnaireListItem HightlightTitleInListItem(QuestionnaireListItem x, string searchTerm)
        {
            var index = x.Title.IndexOf(searchTerm, StringComparison.CurrentCultureIgnoreCase);

            var title = index >= 0
                ? Regex.Replace(x.Title, searchTerm, "<b>" + searchTerm + "</b>", RegexOptions.IgnoreCase)
                : x.Title;

            return new QuestionnaireListItem
                   {
                       Id = x.Id,
                       IsPublic = x.IsPublic,
                       LastEntryDate = x.LastEntryDate,
                       OwnerName = x.OwnerName,
                       Title = title
                   };
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}