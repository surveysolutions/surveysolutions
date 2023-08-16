using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class DashboardViewModel : BasePrincipalViewModel
    {
        private CancellationTokenSource tokenSource;
        private readonly ITesterPrincipal principal;
        private readonly IDesignerApiService designerApiService;

        private readonly IUserInteractionService userInteractionService;
        private IReadOnlyCollection<QuestionnaireListItem> localQuestionnaires = new List<QuestionnaireListItem>();
        private readonly IPlainStorage<QuestionnaireListItem> questionnaireListStorage;
        private readonly IPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage;
        private readonly ILogger logger;
        private readonly IFriendlyErrorMessageService friendlyErrorMessageService;

        private QuestionnaireDownloadViewModel QuestionnaireDownloader { get; }
        private QuestionnairesType showedQuestionnairesType = QuestionnairesType.My;

        public DashboardViewModel(
            ITesterPrincipal principal,
            IDesignerApiService designerApiService,
            IViewModelNavigationService viewModelNavigationService,
            IFriendlyErrorMessageService friendlyErrorMessageService,
            IUserInteractionService userInteractionService,
            IPlainStorage<QuestionnaireListItem> questionnaireListStorage, 
            IPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage,
            ILogger logger,
            QuestionnaireDownloadViewModel questionnaireDownloader)
            : base(principal, viewModelNavigationService)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
            this.friendlyErrorMessageService = friendlyErrorMessageService;
            this.userInteractionService = userInteractionService;
            this.questionnaireListStorage = questionnaireListStorage;
            this.dashboardLastUpdateStorage = dashboardLastUpdateStorage;
            this.logger = logger;
            this.QuestionnaireDownloader = questionnaireDownloader;
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            
            this.localQuestionnaires = this.questionnaireListStorage.LoadAll();
            
            if (!localQuestionnaires.Any())
#pragma warning disable 4014
                Task.Run(this.LoadServerQuestionnairesAsync);
#pragma warning restore 4014
            else
                this.SearchByLocalQuestionnaires(QuestionnairesType.My);

            this.ShowEmptyQuestionnaireListText = true;
            this.IsSearchVisible = false;

            var lastUpdate = this.dashboardLastUpdateStorage.GetById(this.Principal.CurrentUserIdentity.Name);

            this.HumanizeLastUpdateDate(lastUpdate?.LastUpdateDate);
        }
       
        private void SearchByLocalQuestionnaires(QuestionnairesType type, string searchTerm = null)
        {
            this.showedQuestionnairesType = type;

            var filteredBySearchTerm = this.localQuestionnaires.Where(x => this.HasSearchTerm(searchTerm, x)).ToArray();

            this.MyQuestionnairesCount = filteredBySearchTerm.Count(x => x.IsOwner);
            this.PublicQuestionnairesCount = filteredBySearchTerm.Count(x => x.IsPublic);
            this.SharedWithMeCount = filteredBySearchTerm.Count(x => x.IsShared);

            this.Questionnaires = filteredBySearchTerm
                .Where(this.FilterByType)
                .Select(x => ToViewModel(x, searchTerm))
                .OrderByDescending(questionnaire => questionnaire.LastEntryDate)
                .ToList();

            this.IsListEmpty = !this.Questionnaires.Any();
        }

        private QuestionnaireListItemViewModel ToViewModel(QuestionnaireListItem item, string searchTerm) =>
            new QuestionnaireListItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                LastEntryDate = item.LastEntryDate,
                OwnerName = item.OwnerName,
                IsOwner = item.IsOwner,
                IsPublic = item.IsPublic,
                IsShared = item.IsShared,
                SearchTerm = searchTerm,
                Type = this.showedQuestionnairesType
            };

        private bool FilterByType(QuestionnaireListItem x)
        {
            switch (this.showedQuestionnairesType)
            {
                case QuestionnairesType.My when !x.IsOwner:
                case QuestionnairesType.SharedWithMe when !x.IsShared:
                case QuestionnairesType.Public when !x.IsPublic:
                    return false;
            }

            return true;
        }

        private bool HasSearchTerm(string searchTerm, QuestionnaireListItem x)
        {
            if (string.IsNullOrEmpty(searchTerm)) return true;

            var titleHasSearchTerm = CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Title, searchTerm, CompareOptions.IgnoreCase) >= 0;
            var ownerNameHasSearchTerm = x.OwnerName?.Contains(searchTerm) ?? false;

            return titleHasSearchTerm || ownerNameHasSearchTerm;
        }

        private string humanizedLastUpdateDate;
        public string HumanizedLastUpdateDate
        {
            get => this.humanizedLastUpdateDate;
            set => this.SetProperty(ref this.humanizedLastUpdateDate, value);
        }

        private IList<QuestionnaireListItemViewModel> questionnaires = new List<QuestionnaireListItemViewModel>();
        public IList<QuestionnaireListItemViewModel> Questionnaires
        {
            get => this.questionnaires;
            set => this.SetProperty(ref this.questionnaires, value);
        }

        private bool showEmptyQuestionnaireListText;
        public bool ShowEmptyQuestionnaireListText
        {
            get => this.showEmptyQuestionnaireListText;
            set => this.SetProperty(ref this.showEmptyQuestionnaireListText, value);
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => isInProgress;
            set => this.SetProperty(ref this.isInProgress, value);
        }

        private string progressIndicator;
        public string ProgressIndicator
        {
            get => progressIndicator;
            set => this.SetProperty(ref this.progressIndicator, value);
        }

        private int myQuestionnairesCount;
        public int MyQuestionnairesCount
        {
            get => myQuestionnairesCount;
            set => this.SetProperty(ref this.myQuestionnairesCount, value);
        }

        private int publicQuestionnairesCount;
        public int PublicQuestionnairesCount
        {
            get => publicQuestionnairesCount;
            set => this.SetProperty(ref this.publicQuestionnairesCount, value);
        }

        private int sharedWithMeCount;
        public int SharedWithMeCount
        {
            get => sharedWithMeCount;
            set => this.SetProperty(ref this.sharedWithMeCount, value);
        }

        private bool isSearchVisible;
        public bool IsSearchVisible
        {
            get => isSearchVisible;
            set => this.SetProperty(ref this.isSearchVisible, value);
        }

        private bool isListEmpty;
        public bool IsListEmpty
        {
            get => this.isListEmpty;
            set => this.SetProperty(ref this.isListEmpty, value);
        }

        private string searchText;
        public string SearchText
        {
            get => this.searchText;
            set => this.SetProperty(ref this.searchText, value);
        }

        public IMvxCommand ClearSearchCommand => new MvxCommand(this.ClearSearch);

        public IMvxCommand ShowSearchCommand => new MvxCommand(this.ShowSearch);

        public IMvxCommand SearchCommand => new MvxCommand<string>(x =>
            this.SearchByLocalQuestionnaires(this.showedQuestionnairesType, x));

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

        public IMvxAsyncCommand<QuestionnaireListItemViewModel> LoadQuestionnaireCommand =>
            new MvxAsyncCommand<QuestionnaireListItemViewModel>(this.LoadQuestionnaireAsync, _ => !this.IsInProgress);

        public IMvxAsyncCommand RefreshQuestionnairesCommand => new MvxAsyncCommand(this.LoadServerQuestionnairesAsync, () => !this.IsInProgress);

        public IMvxCommand ShowMyQuestionnairesCommand => new MvxCommand(() =>
            this.SearchByLocalQuestionnaires(QuestionnairesType.My, this.SearchText));

        public IMvxCommand ShowPublicQuestionnairesCommand => new MvxCommand(() =>
            this.SearchByLocalQuestionnaires(QuestionnairesType.Public, this.SearchText));
        public IMvxCommand ShowSharedWithMeCommand => new MvxCommand(() =>
            this.SearchByLocalQuestionnaires(QuestionnairesType.SharedWithMe, this.SearchText));

        private void ShowSearch()
        {
            if (IsInProgress)
                return;
            IsSearchVisible = true;
        }

        private void ClearSearch()
        {
            this.SearchText = null;
            IsSearchVisible = false;
        }

        private Task SignOut()
        {
            this.CancelLoadServerQuestionnaires();
            return this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync();
        }

        private async Task LoadQuestionnaireAsync(QuestionnaireListItemViewModel questionnaireListItem)
        {
            if (this.IsInProgress) return;

            this.tokenSource = new CancellationTokenSource();
            this.IsInProgress = true;

            var progress = new Progress<string>();

            try
            {
                await this.QuestionnaireDownloader
                    .LoadQuestionnaireAsync(questionnaireListItem.Id, questionnaireListItem.Title, progress,
                        this.tokenSource.Token);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private async Task LoadServerQuestionnairesAsync()
        {
            this.IsInProgress = true;
            this.tokenSource = new CancellationTokenSource();
            try
            {
                this.ClearSearch();

                this.questionnaireListStorage.Remove(this.localQuestionnaires);

                this.localQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(token: tokenSource.Token);
                
                this.questionnaireListStorage.Store(this.localQuestionnaires);

                var lastUpdateDate = DateTime.UtcNow;
                this.HumanizeLastUpdateDate(lastUpdateDate);

                this.dashboardLastUpdateStorage.Store(new DashboardLastUpdate
                {
                    Id = this.Principal.CurrentUserIdentity.Name,
                    LastUpdateDate = lastUpdateDate
                });

                this.SearchByLocalQuestionnaires(this.showedQuestionnairesType);
            }
            catch (RestException ex)
            {
                if (ex.Type == RestExceptionType.RequestCanceledByUser)
                    return;

                var errorMessage = this.friendlyErrorMessageService.GetFriendlyErrorMessageByRestException(ex);

                if (!string.IsNullOrEmpty(errorMessage))
                    await this.userInteractionService.AlertAsync(errorMessage);
                else throw;
            }
            catch (Exception ex)
            {
                this.logger.Error("Load questionnaire list exception. ", ex);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private void HumanizeLastUpdateDate(DateTime? lastUpdate)
        {
            this.HumanizedLastUpdateDate = lastUpdate.HasValue
                ? string.Format(TesterUIResources.Dashboard_LastUpdated, lastUpdate.Value.Humanize(dateToCompareAgainst: DateTime.UtcNow, culture: CultureInfo.InvariantCulture))
                : TesterUIResources.Dashboard_HaveNotBeenUpdated;
        }

        public void CancelLoadServerQuestionnaires()
        {
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
            {
                this.tokenSource.Cancel();
            }
        }
        
        public override void ViewAppearing()
        {
            base.ViewAppearing();
            
            if (principal.IsFakeIdentity)
                ViewModelNavigationService.NavigateToLoginAsync();
        }
    }
}
