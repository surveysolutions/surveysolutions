using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class SearchViewModel : BaseViewModel, IDisposable
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;

        private readonly IDisposable startingLongOperationMessageSubscriptionToken;
        private readonly IDisposable stopLongOperationMessageSubscriptionToken;

        public SearchViewModel(IPrincipal principal, 
            IViewModelNavigationService viewModelNavigationService,
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IAssignmentDocumentsStorage assignmentsRepository,
            IMvxMessenger messenger
            ) : base(principal, viewModelNavigationService)
        {
            this.viewModelFactory = viewModelFactory;
            this.interviewViewRepository = interviewViewRepository;
            this.identifyingQuestionsRepo = identifyingQuestionsRepo;
            this.assignmentsRepository = assignmentsRepository;

            startingLongOperationMessageSubscriptionToken = messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken = messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);
        }

        public bool IsNeedFocus { get; set; } = true;
        public string EmptySearchText { get; private set; }

        private string searchText;
        public string SearchText
        {
            get => this.searchText;
            set => SetProperty(ref this.searchText, value);
        }

        private string serchResultText;
        public string SerchResultText 
        {
            get => this.serchResultText;
            set => SetProperty(ref this.serchResultText, value);
        }

        private bool isInProgressLongOperation;
        public bool IsInProgressLongOperation
        {
            get => this.isInProgressLongOperation;
            set => SetProperty(ref this.isInProgressLongOperation, value);
        }

        private bool isInProgressItemsLoading;
        public bool IsInProgressItemsLoading
        {
            get => this.isInProgressItemsLoading;
            set => SetProperty(ref this.isInProgressItemsLoading, value);
        }

        public IMvxCommand ClearSearchCommand => new MvxCommand(() => SearchText = string.Empty, () => !IsInProgressLongOperation && !IsInProgressItemsLoading);
        public IMvxAsyncCommand ExitSearchCommand => new MvxAsyncCommand(() => viewModelNavigationService.NavigateToDashboardAsync());
        public IMvxAsyncCommand<string> SearchCommand => new MvxAsyncCommand<string>(Search, _ => !IsInProgressLongOperation);

        private async Task Search(string searctText)
        {
            await UpdateUiItemsAsync(searctText);
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            EmptySearchText = InterviewerUIResources.Dashboard_SearchWatermark;
            await UpdateUiItemsAsync(SearchText);
        }

        private List<InterviewView> interviews;
        private List<InterviewView> GetInterviewItems()
            => interviews ?? (interviews = this.interviewViewRepository.LoadAll().ToList());

        private ILookup<Guid, PrefilledQuestionView> identifyingQuestions;
        private ILookup<Guid, PrefilledQuestionView> GetIdentifyingQuestions()
            => identifyingQuestions ?? (identifyingQuestions = this.identifyingQuestionsRepo.LoadAll().ToLookup(d => d.InterviewId));

        private IReadOnlyCollection<AssignmentDocument> assignments;
        private IReadOnlyCollection<AssignmentDocument> GetAssignmentItems()
            => assignments ?? (assignments = this.assignmentsRepository.LoadAll());

        private MvxObservableCollection<IDashboardItem> uiItems = new MvxObservableCollection<IDashboardItem>();
        public MvxObservableCollection<IDashboardItem> UiItems
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        protected Task UpdateUiItemsAsync(string searctText) => Task.Run(() =>
        {
            this.IsInProgressItemsLoading = true;

            try
            {
                if (string.IsNullOrWhiteSpace(searctText))
                {
                    SerchResultText = InterviewerUIResources.Dashboard_NeedTextForSearch;

                    this.UiItems.OfType<InterviewDashboardItemViewModel>().ForEach(i => i.OnItemRemoved -= InterviewItemRemoved);
                    this.UiItems.SwitchTo(new List<InterviewDashboardItemViewModel>());
                }
                else
                {
                    var items = this.GetUiItems(searctText).ToList();
                    SerchResultText = items.Count > 0
                        ? string.Format(InterviewerUIResources.Dashboard_SearchResult, items.Count)
                        : InterviewerUIResources.Dashboard_NotFoundSearchResult;

                    this.UiItems.OfType<InterviewDashboardItemViewModel>().ForEach(i => i.OnItemRemoved -= InterviewItemRemoved);
                    this.UiItems.ReplaceWith(items);
                    this.UiItems.OfType<InterviewDashboardItemViewModel>().ForEach(i => i.OnItemRemoved += InterviewItemRemoved);
                }
            }
            finally
            {
                this.IsInProgressItemsLoading = false;
            }
        });

        private void InterviewItemRemoved(object sender, EventArgs eventArgs)
        {
            var item = (InterviewDashboardItemViewModel)sender;
            item.OnItemRemoved -= InterviewItemRemoved;

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
            SerchResultText = countOfItems > 0
                ? string.Format(InterviewerUIResources.Dashboard_SearchResult, countOfItems)
                : InterviewerUIResources.Dashboard_NotFoundSearchResult;
        }
        
        protected IEnumerable<IDashboardItem> GetUiItems(string searctText)
        {
            foreach (var assignmentItem in GetAssignmentItems())
            {
                if (assignmentItem.Quantity.HasValue && assignmentItem.CreatedInterviewsCount.HasValue)
                {
                    int count = assignmentItem.Quantity.Value - assignmentItem.CreatedInterviewsCount.Value;
                    if (count == 0)
                        continue;
                }

                bool isMatched = Contains(assignmentItem.Title, searctText)
                                 || Contains(assignmentItem.Id.ToString(), searctText)
                                 || (assignmentItem.IdentifyingAnswers?.Any(pi => Contains(pi.AnswerAsString, searctText)) ?? false);

                if (isMatched)
                {
                    var assignmentItemViewModel = this.viewModelFactory.GetDashboardAssignment(assignmentItem);
                    yield return assignmentItemViewModel;
                }
            }


            var preffilledQuestions = GetIdentifyingQuestions();

            foreach (var interviewView in this.GetInterviewItems())
            {
                var details = preffilledQuestions[interviewView.InterviewId]
                    .OrderBy(x => x.SortIndex)
                    .Select(fi => new PrefilledQuestion { Answer = fi.Answer?.Trim(), Question = fi.QuestionText })
                    .ToList();

                bool isMatched = 
                                 Contains(interviewView.InterviewKey, searctText)
                                 || Contains(interviewView.QuestionnaireTitle, searctText)
                                 || Contains(interviewView.Assignment?.ToString(), searctText)
                                 || Contains(interviewView.LastInterviewerOrSupervisorComment, searctText)
                                 || details.Any(pi => Contains(pi.Answer, searctText));

                if (isMatched)
                {
                    var interviewDashboardItem = this.viewModelFactory.GetDashboardInterview(interviewView, details);
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


        public void Dispose()
        {
            startingLongOperationMessageSubscriptionToken.Dispose();
            stopLongOperationMessageSubscriptionToken.Dispose();

            this.UiItems?.OfType<InterviewDashboardItemViewModel>().ForEach(i => i.OnItemRemoved -= InterviewItemRemoved);

            interviews = null;
            identifyingQuestions = null;
            assignments = null;
        }
    }
}
