using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardSearchViewModel : BaseViewModel, IDisposable
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;

        private readonly IDisposable startingLongOperationMessageSubscriptionToken;
        private readonly IDisposable stopLongOperationMessageSubscriptionToken;

        public DashboardSearchViewModel(IPrincipal principal, 
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
            this.viewModelNavigationService = viewModelNavigationService;
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

        public IMvxCommand ClearSearchCommand => new MvxCommand(() => SearchText = string.Empty);
        public IMvxCommand ExitSearchCommand => new MvxAsyncCommand(async () => await viewModelNavigationService.NavigateToDashboardAsync());
        public IMvxCommand SearchCommand => new MvxCommand<string>(Search);

        private void Search(string searctText)
        {
            UpdateUiItems(searctText);
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            EmptySearchText = InterviewerUIResources.Dashboard_SearchWatermark;
            UpdateUiItems(SearchText);
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

        public event EventHandler OnItemsLoaded;

        private MvxObservableCollection<IDashboardItem> uiItems = new MvxObservableCollection<IDashboardItem>();
        public MvxObservableCollection<IDashboardItem> UiItems
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        protected void UpdateUiItems(string searctText) => Task.Run(() =>
        {
            this.IsInProgressItemsLoading = true;

            try
            {
                List<IDashboardItem> items = new List<IDashboardItem>();

                if (string.IsNullOrWhiteSpace(searctText))
                {
                    SerchResultText = InterviewerUIResources.Dashboard_NeedTextForSearch;
                }
                else
                {
                    var newItems = this.GetUiItems(searctText);
                    items.AddRange(newItems);
                    var countOfItems = items.Count;
                    SerchResultText = countOfItems > 0
                        ? string.Format(InterviewerUIResources.Dashboard_SearchResult, countOfItems)
                        : InterviewerUIResources.Dashboard_NotFoundSearchResult;
                }

                this.UiItems.OfType<InterviewDashboardItemViewModel>().ForEach(i => i.OnItemRemoved -= InterviewItemRemoved);
                this.UiItems.ReplaceWith(items);
                this.UiItems.OfType<InterviewDashboardItemViewModel>().ForEach(i => i.OnItemRemoved += InterviewItemRemoved);
            }
            finally
            {
                this.IsInProgressItemsLoading = false;
            }

            this.OnItemsLoaded?.Invoke(this, EventArgs.Empty);
        });

        private void InterviewItemRemoved(object sender, EventArgs eventArgs)
        {
            var item = (InterviewDashboardItemViewModel)sender;
            item.OnItemRemoved -= InterviewItemRemoved;

            if (item.AssignmentId.HasValue)
            {
                assignmentsRepository.DecreaseInterviewsCount(item.AssignmentId.Value);

                this.UiItems
                    .OfType<InterviewerAssignmentDashboardItemViewModel>()
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
                bool isMatched = Contatins(assignmentItem.Title, searctText)
                                 || Contatins(assignmentItem.Id.ToString(), searctText)
                                 || (assignmentItem.IdentifyingAnswers?.Any(pi => Contatins(pi.AnswerAsString, searctText)) ?? false);

                if (isMatched)
                {
                    var assignmentItemViewModel = this.viewModelFactory.GetNew<InterviewerAssignmentDashboardItemViewModel>();
                    assignmentItemViewModel.Init(assignmentItem);
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

                bool isMatched = interviewView.ResponsibleId == principal.CurrentUserIdentity.UserId
                                 &&
                                 (
                                     Contatins(interviewView.InterviewKey, searctText)
                                     || Contatins(interviewView.QuestionnaireTitle, searctText)
                                     || Contatins(interviewView.Assignment?.ToString(), searctText)
                                     || Contatins(interviewView.LastInterviewerOrSupervisorComment, searctText)
                                     || details.Any(pi => Contatins(pi.Answer, searctText))
                                 );

                if (isMatched)
                {
                    var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                    interviewDashboardItem.Init(interviewView, details);
                    yield return interviewDashboardItem;
                }
            }
        }

        private bool Contatins(string originalString, string searchText)
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
