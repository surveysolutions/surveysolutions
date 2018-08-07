using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private CancellationTokenSource tokenSource;
        private readonly IDesignerApiService designerApiService;

        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private IReadOnlyCollection<QuestionnaireListItem> localQuestionnaires = new List<QuestionnaireListItem>();
        private readonly IPlainStorage<QuestionnaireListItem> questionnaireListStorage;
        private readonly IPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage;
        private readonly ILogger logger;
        private readonly IAsyncRunner asyncRunner;
        private readonly IFriendlyErrorMessageService friendlyErrorMessageService;

        private QuestionnaireDownloadViewModel QuestionnaireDownloader { get; }

        public DashboardViewModel(
            IPrincipal principal,
            IDesignerApiService designerApiService,
            IViewModelNavigationService viewModelNavigationService,
            IFriendlyErrorMessageService friendlyErrorMessageService,
            IUserInteractionService userInteractionService,
            IPlainStorage<QuestionnaireListItem> questionnaireListStorage, 
            IPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage,
            ILogger logger,
            IAsyncRunner asyncRunner,
            QuestionnaireDownloadViewModel questionnaireDownloader)
            : base(principal, viewModelNavigationService)
        {
            this.designerApiService = designerApiService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.friendlyErrorMessageService = friendlyErrorMessageService;
            this.userInteractionService = userInteractionService;
            this.questionnaireListStorage = questionnaireListStorage;
            this.dashboardLastUpdateStorage = dashboardLastUpdateStorage;
            this.logger = logger;
            this.asyncRunner = asyncRunner;
            this.QuestionnaireDownloader = questionnaireDownloader;
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            this.localQuestionnaires = this.questionnaireListStorage.LoadAll();
            
            if (!localQuestionnaires.Any())
            {
                this.asyncRunner.RunAsync(this.LoadServerQuestionnairesAsync);
            }
            else
            {
                this.SearchByLocalQuestionnaires();
            }

            this.ShowEmptyQuestionnaireListText = true;
            this.IsSearchVisible = false;

            var lastUpdate = this.dashboardLastUpdateStorage.GetById(this.principal.CurrentUserIdentity.Name);

            this.HumanizeLastUpdateDate(lastUpdate?.LastUpdateDate);
        }
       
        private void SearchByLocalQuestionnaires(string searchTerm = null)
        {
            var trimmedSearchText = (searchTerm ?? "").Trim();

            if (searchTerm == "crash")
            {
                throw new InvalidOperationException("KABOOOOOOM");
            }
            bool EmptyFilter(QuestionnaireListItem x) => true;

            bool TitleSearchFilter(QuestionnaireListItem x) => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Title, trimmedSearchText, CompareOptions.IgnoreCase) >= 0 || (x.OwnerName != null && x.OwnerName.Contains(trimmedSearchText));

            Func<QuestionnaireListItem, bool> searchFilter = string.IsNullOrEmpty(trimmedSearchText)
                ? (Func<QuestionnaireListItem, bool>) EmptyFilter
                : TitleSearchFilter;

            var myQuestionnaires = this.localQuestionnaires
                .Where(questionnaire =>
                    searchFilter(questionnaire)
                    &&
                    (
                        questionnaire.IsOwner || questionnaire.IsShared
                    ))
                .ToList();

            var publicQuestionnaires = this.localQuestionnaires
                .Where(questionnaire => searchFilter(questionnaire) && questionnaire.IsPublic)
                .ToList();

            var selectedQuestionnaires = this.IsPublicShowed ? publicQuestionnaires : myQuestionnaires;

            this.MyQuestionnairesCount = myQuestionnaires.Count;
            this.PublicQuestionnairesCount = publicQuestionnaires.Count;

            this.IsListEmpty = !selectedQuestionnaires.Any();

            this.Questionnaires = selectedQuestionnaires
                .Select(questionnaire => ToDashboardQuestionnaire(questionnaire, searchTerm))
                .OrderByDescending(questionnaire => questionnaire.LastEntryDate)
                .ToList();
        }

        private string humanizedLastUpdateDate;
        public string HumanizedLastUpdateDate
        {
            get { return this.humanizedLastUpdateDate; }
            set { this.humanizedLastUpdateDate = value; RaisePropertyChanged(); }
        }

        private IList<QuestionnaireListItem> questionnaires = new List<QuestionnaireListItem>();
        public IList<QuestionnaireListItem> Questionnaires
        {
            get { return this.questionnaires; }
            set { this.questionnaires = value; RaisePropertyChanged(); }
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

        public bool IsListEmpty
        {
            get { return this.isListEmpty; }
            set { this.isListEmpty = value; RaisePropertyChanged(); }
        }
        
        public IMvxCommand ClearSearchCommand => new MvxCommand(this.ClearSearch);

        public IMvxCommand ShowSearchCommand => new MvxCommand(this.ShowSearch);

        public IMvxCommand SearchCommand => new MvxCommand<string>(this.SearchByLocalQuestionnaires);

        public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

        private IMvxAsyncCommand<QuestionnaireListItem> loadQuestionnaireCommand;

        public IMvxAsyncCommand<QuestionnaireListItem> LoadQuestionnaireCommand => this.loadQuestionnaireCommand ?? (this.loadQuestionnaireCommand
            = new MvxAsyncCommand<QuestionnaireListItem>(this.LoadQuestionnaireAsync, _ => !this.IsInProgress));


        public IMvxAsyncCommand RefreshQuestionnairesCommand => new MvxAsyncCommand(this.LoadServerQuestionnairesAsync, () => !this.IsInProgress);
        
        public IMvxCommand ShowMyQuestionnairesCommand => new MvxCommand(this.ShowMyQuestionnaires);
        public IMvxCommand ShowPublicQuestionnairesCommand => new MvxCommand(this.ShowPublicQuestionnaires);

        private string searchText;
        public string SearchText
        {
            get { return this.searchText; }
            set { this.searchText = value; RaisePropertyChanged(); }
        }

        private bool isListEmpty;

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
            
            return this.viewModelNavigationService.SignOutAndNavigateToLoginAsync();
        }

        private void ShowPublicQuestionnaires()
        {
            this.IsPublicShowed = true;

            this.SearchByLocalQuestionnaires(this.SearchText);
        }

        private void ShowMyQuestionnaires()
        {
            this.IsPublicShowed = false;

            this.SearchByLocalQuestionnaires(this.SearchText);
        }

        private async Task LoadQuestionnaireAsync(QuestionnaireListItem questionnaireListItem)
        {
            if (this.IsInProgress) return;

            this.tokenSource = new CancellationTokenSource();
            this.IsInProgress = true;

            var progress = new Progress<string>();
            progress.ProgressChanged += this.Progress_ProgressChanged;

            try
            {
                await this.QuestionnaireDownloader.LoadQuestionnaireAsync(questionnaireListItem.Id, questionnaireListItem.Title, progress, this.tokenSource.Token).ConfigureAwait(false);
            }
            finally
            {
                progress.ProgressChanged -= this.Progress_ProgressChanged;
                this.IsInProgress = false;
            }
        }

        private void Progress_ProgressChanged(object sender, string progress) => this.ProgressIndicator = progress;

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
                    Id = this.principal.CurrentUserIdentity.Name,
                    LastUpdateDate = lastUpdateDate
                });

                this.SearchByLocalQuestionnaires();
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

        private QuestionnaireListItem ToDashboardQuestionnaire(QuestionnaireListItem x, string searchTerm)
        {
            return new QuestionnaireListItem
                   {
                       Id = x.Id,
                       IsPublic = this.IsPublicShowed,
                       LastEntryDate = x.LastEntryDate,
                       OwnerName = x.OwnerName,
                       Title = ToDashboardTitle(x.Title, searchTerm)
            };
        }

        private static string ToDashboardTitle(string questionnaireTitle, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return questionnaireTitle;

            var index = CultureInfo.CurrentCulture.CompareInfo.IndexOf(questionnaireTitle, searchTerm, CompareOptions.IgnoreCase);

            string title;
            if (index >= 0)
            {
                var substringToHightlight =  questionnaireTitle.Substring(index, searchTerm.Length);
                title = Regex.Replace(questionnaireTitle, Regex.Escape(searchTerm), "<b>" + substringToHightlight + "</b>", RegexOptions.IgnoreCase);
            }
            else
            {
                title = questionnaireTitle;
            }
            return title;
        }

        public void CancelLoadServerQuestionnaires()
        {
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
            {
                this.tokenSource.Cancel();
            }
        }
    }
}
