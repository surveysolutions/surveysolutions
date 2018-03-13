using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardSearchViewModel : BaseViewModel, IDisposable
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IMvxMessenger messenger;

        private MvxSubscriptionToken startingLongOperationMessageSubscriptionToken;
        private MvxSubscriptionToken stopLongOperationMessageSubscriptionToken;

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
            this.messenger = messenger;
        }

        public string SearchText { get; set; }
        public bool IsNeedFocus { get; set; } = true;
        public string EmptySearchText { get; private set; }

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

        public IMvxCommand ClearSearchCommand => new MvxCommand(() => viewModelNavigationService.NavigateToDashboard());
        public IMvxCommand SearchCommand => new MvxCommand<string>(Search);

        private void Search(string searctText)
        {
            SearchText = searctText;
            UpdateUiItems(searctText);
        }

        public override void Load()
        {
            startingLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StartingLongOperationMessage>(this.DashboardItemOnStartingLongOperation);
            stopLongOperationMessageSubscriptionToken = this.messenger.Subscribe<StopingLongOperationMessage>(this.DashboardItemOnStopLongOperation);

            EmptySearchText = InterviewerUIResources.Dashboard_SearchWatermark;
            UpdateUiItems(SearchText);

            base.Load();
        }

        private IReadOnlyCollection<InterviewView> interviews;
        private IReadOnlyCollection<InterviewView> GetInterviewItems()
            => interviews ?? (interviews = this.interviewViewRepository.LoadAll());

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

        private int itemsCount;
        public int ItemsCount
        {
            get => this.itemsCount;
            protected set => this.RaiseAndSetIfChanged(ref this.itemsCount, value);
        }

        protected void UpdateUiItems(string searctText) => Task.Run(() =>
        {
            this.IsInProgressItemsLoading = true;

            try
            {
                List<IDashboardItem> items = new List<IDashboardItem>();
                var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
                items.Add(subTitle);

                if (string.IsNullOrWhiteSpace(searctText))
                {
                    subTitle.Title = InterviewerUIResources.Dashboard_NeedTextForSearch;
                }
                else
                {
                    var newItems = this.GetUiItems(searctText);
                    items.AddRange(newItems);
                    var countOfItems = items.Count - 1;
                    subTitle.Title = countOfItems > 0
                        ? string.Format(InterviewerUIResources.Dashboard_SearchResult, items.Count - 1)
                        : InterviewerUIResources.Dashboard_NotFoundSearchResult;
                }

                this.UiItems.ReplaceWith(items);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                this.IsInProgressItemsLoading = false;
            }

            this.OnItemsLoaded?.Invoke(this, EventArgs.Empty);
        });
        

        protected IEnumerable<IDashboardItem> GetUiItems(string searctText)
        {
            foreach (var assignmentItem in GetAssignmentItems())
            {
                bool isMatched = assignmentItem.Title.Contains(searctText)
                                 || assignmentItem.Id.ToString().Contains(searctText)
                                 || (assignmentItem.Answers?.Any(pi => pi.AnswerAsString?.Contains(searctText) ?? false) ?? false);

                if (isMatched)
                {
                    var assignmentItemViewModel = this.viewModelFactory.GetNew<AssignmentDashboardItemViewModel>();
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
                                     interviewView.InterviewKey.Contains(searctText)
                                     || (interviewView.Assignment?.ToString().Contains(searctText) ?? false)
                                     || (interviewView.LastInterviewerOrSupervisorComment?.Contains(searctText) ?? false)
                                     || details.Any(pi => pi.Answer?.Contains(searctText) ?? false)
                                 );

                if (isMatched)
                {
                    var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                    interviewDashboardItem.Init(interviewView, details);

                    //this.OnItemCreated(interviewDashboardItem);

                    yield return interviewDashboardItem;
                }
            }
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
            messenger.Unsubscribe<StartingLongOperationMessage>(startingLongOperationMessageSubscriptionToken);
            messenger.Unsubscribe<StopingLongOperationMessage>(stopLongOperationMessageSubscriptionToken);

            interviews = null;
            identifyingQuestions = null;
            assignments = null;
        }
    }
}
