using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Messages;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class SearchViewModel : BaseAuthenticatedViewModel
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher;

        private readonly IDisposable startingLongOperationMessageSubscriptionToken;
        private readonly IDisposable stopLongOperationMessageSubscriptionToken;
        private CancellationTokenSource cancellationTokenSource;

        public SearchViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IAssignmentDocumentsStorage assignmentsRepository,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher
        ) : base(principal, viewModelNavigationService)
        {
            this.viewModelFactory = viewModelFactory;
            this.interviewViewRepository = interviewViewRepository;
            this.identifyingQuestionsRepo = identifyingQuestionsRepo;
            this.assignmentsRepository = assignmentsRepository;
            this.mainThreadAsyncDispatcher = mainThreadAsyncDispatcher;

            var messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            startingLongOperationMessageSubscriptionToken = 
                messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken = 
                messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
        }

        public bool IsNeedFocus { get; set; } = true;
        public string EmptySearchText { get; private set; }

        private string searchTextField;
        public string SearchText
        {
            get => this.searchTextField;
            set => SetProperty(ref this.searchTextField, value);
        }

        private string searchResultText;
        public string SearchResultText 
        {
            get => this.searchResultText;
            set => SetProperty(ref this.searchResultText, value);
        }

        private bool isInProgressLongOperation;
        public bool IsInProgressLongOperation
        {
            get => this.isInProgressLongOperation;
            set => SetProperty(ref this.isInProgressLongOperation, value);
        }

        private int isInProgressItemsLoadingCount;
        private int IsInProgressItemsLoadingCount
        {
            get => this.isInProgressItemsLoadingCount;
            set
            {
                this.isInProgressItemsLoadingCount = value;
                RaisePropertyChanged(nameof(IsInProgressItemsLoading));
            }
        }

        public bool IsInProgressItemsLoading => this.IsInProgressItemsLoadingCount > 0;

        public IMvxCommand ClearSearchCommand => new MvxCommand(
            () => SearchText = string.Empty, 
            () => !IsInProgressLongOperation);
        public IMvxCommand ExitSearchCommand => new MvxAsyncCommand(() => ViewModelNavigationService.NavigateToDashboardAsync());
        public IMvxCommand<string> SearchCommand => new MvxCommand<string>(async text => await SearchAsync(text));

        public async Task SearchAsync(string searchText)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            await UpdateUiItemsAsync(searchText, cancellationTokenSource.Token);
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            EmptySearchText = EnumeratorUIResources.Dashboard_SearchWatermark;
            await UpdateUiItemsAsync(SearchText, CancellationToken.None);
        }

        private List<InterviewView> interviews;
        private List<InterviewView> GetInterviewItems()
            => interviews ??= this.interviewViewRepository.LoadAll().ToList();

        private ILookup<Guid, PrefilledQuestionView> identifyingQuestionsField;
        private ILookup<Guid, PrefilledQuestionView> GetIdentifyingQuestions()
            => identifyingQuestionsField ??= this.identifyingQuestionsRepo.LoadAll().ToLookup(d => d.InterviewId);

        private IReadOnlyCollection<AssignmentDocument> assignments;
        private IReadOnlyCollection<AssignmentDocument> GetAssignmentItems()
            => assignments ??= this.assignmentsRepository.LoadAll()
                .Where(assignment =>
                    !assignment.Quantity.HasValue 
                    || !assignment.CreatedInterviewsCount.HasValue
                    || assignment.Quantity.Value - assignment.CreatedInterviewsCount.Value > 0
                )
                .ToReadOnlyCollection()
                .SortAssignments();

        private MvxObservableCollection<IDashboardItem> uiItems = new MvxObservableCollection<IDashboardItem>();
        public MvxObservableCollection<IDashboardItem> UiItems
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        protected async Task UpdateUiItemsAsync(string searchText, CancellationToken cancellationToken)
        {
            this.IsInProgressItemsLoadingCount++;

            this.SearchResultText = string.IsNullOrWhiteSpace(searchText)
                ? EnumeratorUIResources.Dashboard_NeedTextForSearch
                : EnumeratorUIResources.Dashboard_Searching;

            var items = new List<IDashboardItem>();

            try
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    items = await this.GetViewModelsAsync(searchText, cancellationToken);

                    this.SearchResultText = items.Count > 0
                        ? string.Format(EnumeratorUIResources.Dashboard_SearchResult, items.Count)
                        : EnumeratorUIResources.Dashboard_NotFoundSearchResult;
                }

                this.UiItems.OfType<InterviewDashboardItemViewModel>().ForEach(i =>
                {
                    i.OnItemRemoved -= InterviewItemRemoved;
                    if (i is IDashboardItemWithEvents withEvents)
                        withEvents.OnItemUpdated -= OnItemUpdated;
                });
                await mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(()=>this.UiItems.ReplaceWith(items));
                this.UiItems.OfType<InterviewDashboardItemViewModel>().ForEach(i =>
                {
                    i.OnItemRemoved += InterviewItemRemoved;
                    if (i is IDashboardItemWithEvents withEvents)
                        withEvents.OnItemUpdated += OnItemUpdated;
                });
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                this.IsInProgressItemsLoadingCount--;
            }
        }

        protected virtual void OnItemUpdated(object sender, EventArgs args)
        {
            if (sender is AssignmentDashboardItemViewModel assignmentModel)
            {
                var assignment = this.assignmentsRepository.GetById(assignmentModel.AssignmentId);
                assignmentModel.Init(assignment);
            }

            if (sender is InterviewDashboardItemViewModel interviewModel)
            {
                var interviewId = interviewModel.InterviewId;
                var updatedView = interviewViewRepository.GetById(interviewId.FormatGuid());
                var details = this.identifyingQuestionsRepo
                    .Where(p => p.InterviewId == interviewId)
                    .OrderBy(x => x.SortIndex)
                    .Select(fi => new PrefilledQuestion {Answer = fi.Answer?.Trim(), Question = fi.QuestionText})
                    .ToList();

                interviewModel.Init(updatedView, details);
            }
        }


        private Task<List<IDashboardItem>> GetViewModelsAsync(string searchText, CancellationToken cancellationToken) =>
            Task.Run(() =>
            {
                var items = new List<IDashboardItem>();

                foreach (var uiItem in this.GetUiItems(searchText))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    items.Add(uiItem);
                }

                return items;

            }, cancellationToken);

        private void InterviewItemRemoved(object sender, EventArgs eventArgs)
        {
            var item = (InterviewDashboardItemViewModel)sender;
            item.OnItemRemoved -= InterviewItemRemoved;
            item.OnItemUpdated -= OnItemUpdated;

            if (item.AssignmentId.HasValue)
            {
                assignmentsRepository.DecreaseInterviewsCount(item.AssignmentId.Value);

                this.UiItems
                    .OfType<AssignmentDashboardItemViewModel>()
                    .FirstOrDefault(x => x.AssignmentId == item.AssignmentId.Value)
                    ?.DecreaseInterviewsCount();
            }

            this.interviews.RemoveAll(x => x.InterviewId == item.InterviewId);

            UiItems.RemoveItems(item.ToEnumerable());

            var countOfItems = UiItems.Count;
            SearchResultText = countOfItems > 0
                ? string.Format(EnumeratorUIResources.Dashboard_SearchResult, countOfItems)
                : EnumeratorUIResources.Dashboard_NotFoundSearchResult;
        }
        
        protected IEnumerable<IDashboardItem> GetUiItems(string searchText)
        {
            foreach (var assignmentItem in GetAssignmentItems())
            {
                bool isMatched = Contains(assignmentItem.Title, searchText)
                                 || Contains(assignmentItem.Id.ToString(), searchText)
                                 || (assignmentItem.IdentifyingAnswers?.Any(pi => Contains(pi.AnswerAsString, searchText)) ?? false)
                                 || Contains(assignmentItem.CalendarEventComment, searchText);

                if (isMatched)
                {
                    var assignmentItemViewModel = this.viewModelFactory.GetDashboardAssignment(assignmentItem);
                    yield return assignmentItemViewModel;
                }
            }


            var identifyingQuestions = GetIdentifyingQuestions();

            foreach (var interviewView in this.GetInterviewItems())
            {
                var details = identifyingQuestions[interviewView.InterviewId];

                bool isMatched = 
                                 Contains(interviewView.InterviewKey, searchText)
                                 || Contains(interviewView.QuestionnaireTitle, searchText)
                                 || Contains(interviewView.Assignment?.ToString(), searchText)
                                 || Contains(interviewView.LastInterviewerOrSupervisorComment, searchText)
                                 || details.Any(pi => Contains(pi.Answer, searchText))
                                 || Contains(interviewView.CalendarEventComment, searchText);

                if (isMatched)
                {
                    var interviewDashboardItem = this.viewModelFactory.GetDashboardInterview(interviewView, 
                        details
                            .OrderBy(x => x.SortIndex)
                            .Select(fi => new PrefilledQuestion { Answer = fi.Answer?.Trim(), Question = fi.QuestionText })
                            .ToList());
                    yield return interviewDashboardItem;
                }
            }
        }

        private bool Contains(string originalString, string searchText)
        {
            if (string.IsNullOrWhiteSpace(originalString))
                return false;

            var indexOf = originalString.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            return indexOf >= 0;
        }

        private void DashboardItemOnStartingLongOperation(StartingLongOperationMessage message)
        {
            IsInProgressLongOperation = true;
        }

        private void DashboardItemOnStopLongOperation(StopingLongOperationMessage message)
        {
            IsInProgressLongOperation = false;
        }

        private bool isDisposed = false;
        
        public override void Dispose()
        {
            if(isDisposed)  return;
            isDisposed = true;
            
            startingLongOperationMessageSubscriptionToken.Dispose();
            stopLongOperationMessageSubscriptionToken.Dispose();

            this.UiItems?.OfType<InterviewDashboardItemViewModel>()
                .ForEach(i => i.OnItemRemoved -= InterviewItemRemoved);

            interviews = null;
            identifyingQuestionsField = null;
            assignments = null;
            
            base.Dispose();
        }
    }
}
