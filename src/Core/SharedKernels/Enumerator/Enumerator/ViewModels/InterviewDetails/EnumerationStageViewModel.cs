using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class EnumerationStageViewModel : MvxViewModel,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<AnswersDeclaredInvalid>,
        ILiteEventHandler<StaticTextsDeclaredInvalid>,
        IDisposable
    {
        private CompositeCollection<ICompositeEntity> items;
        public CompositeCollection<ICompositeEntity> Items
        {
            get { return this.items; }
            set
            {
                this.items = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IEnumeratorSettings settings;
        readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMessenger messenger;
        private readonly ICompositeCollectionInflationService compositeCollectionInflationService;

        readonly IUserInterfaceStateService userInterfaceStateService;
        private readonly IMvxMainThreadDispatcher mvxMainThreadDispatcher;

        private NavigationState navigationState;

        IStatefulInterview interview;
        private IQuestionnaire questionnaire;
        string interviewId;

        public DynamicTextViewModel Name { get; }

        public EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxMainThreadDispatcher mvxMainThreadDispatcher,
            DynamicTextViewModel dynamicTextViewModel, 
            IMvxMessenger messenger, 
            IEnumeratorSettings settings,
            ICompositeCollectionInflationService compositeCollectionInflationService)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
            this.eventRegistry = eventRegistry;
            this.userInterfaceStateService = userInterfaceStateService;
            this.mvxMainThreadDispatcher = mvxMainThreadDispatcher;

            this.Name = dynamicTextViewModel;
            this.messenger = messenger;
            this.settings = settings;
            this.compositeCollectionInflationService = compositeCollectionInflationService;
        }

        public void Init(string interviewId, NavigationState navigationState, Identity groupId, Identity anchoredElementIdentity)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (this.navigationState != null) throw new InvalidOperationException("ViewModel already initialized");

            this.interviewId = interviewId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language);

            this.navigationState = navigationState;
            this.Items = new CompositeCollection<ICompositeEntity>();

            this.InitRegularGroupScreen(groupId, anchoredElementIdentity);

            if (!this.eventRegistry.IsSubscribed(this))
            {
                this.eventRegistry.Subscribe(this, this.interviewId);
            }
        }

        private void InitRegularGroupScreen(Identity groupIdentity, Identity anchoredElementIdentity)
        {
            if (this.questionnaire.IsRosterGroup(groupIdentity.Id))
            {
                string rosterTitle = this.questionnaire.GetGroupTitle(groupIdentity.Id);
                var fullRosterName = this.substitutionService.GenerateRosterName(rosterTitle, this.interview.GetRosterTitle(groupIdentity));
                this.Name.Init(this.interviewId, groupIdentity, fullRosterName);
            }
            else
            {
                var groupTitle = this.questionnaire.GetGroupTitle(groupIdentity.Id);
                this.Name.Init(this.interviewId, groupIdentity, groupTitle);
            }

            this.LoadFromModel(groupIdentity);
            this.SetScrollTo(anchoredElementIdentity);
        }

        private void SetScrollTo(Identity scrollTo)
        {
            var anchorElementIndex = 0;

            if (scrollTo != null)
            {
                this.mvxMainThreadDispatcher.RequestMainThreadAction(() =>
                {
                    ICompositeEntity childItem = this.Items.OfType<GroupViewModel>().FirstOrDefault(x => x.Identity.Equals(scrollTo));

                    if (childItem == null)
                        childItem = this.Items.OfType<QuestionHeaderViewModel>().FirstOrDefault(x => x. Identity.Equals(scrollTo));

                    if (childItem == null)
                        childItem = this.Items.OfType<StaticTextViewModel>().FirstOrDefault(x => x.Identity.Equals(scrollTo));

                    anchorElementIndex = childItem != null ? this.Items.ToList().IndexOf(childItem) : 0;
                });
            }
            this.ScrollToIndex = anchorElementIndex;
        }

        public int? ScrollToIndex { get; set; }

        private void LoadFromModel(Identity groupIdentity)
        {
            try
            {
                this.userInterfaceStateService.NotifyRefreshStarted();

                var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
                previousGroupNavigationViewModel.Init(this.interviewId, groupIdentity, this.navigationState);

                foreach (var interviewItemViewModel in this.Items.OfType<IDisposable>())
                {
                    interviewItemViewModel.Dispose();
                }

                var entities = this.interviewViewModelFactory.GetEntities(
                    interviewId: this.navigationState.InterviewId,
                    groupIdentity: groupIdentity,
                    navigationState: this.navigationState);

                var newGroupItems = entities.Concat(
                        previousGroupNavigationViewModel.ToEnumerable<IInterviewEntityViewModel>()).ToList();

                this.InterviewEntities = newGroupItems;
                
                this.Items = this.compositeCollectionInflationService.GetInflatedCompositeCollection(newGroupItems);
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
            }
        }

        private IList<IInterviewEntityViewModel> InterviewEntities { get; set; }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (this.navigationState.CurrentGroup == null)
                return;

            foreach (ChangedRosterInstanceTitleDto rosterInstance in @event.ChangedInstances)
            {
                if (this.navigationState.CurrentGroup.Equals(rosterInstance.RosterInstance.GetIdentity()))
                {
                    var fullRosterName = this.substitutionService.GenerateRosterName(
                        this.questionnaire.GetGroupTitle(this.navigationState.CurrentGroup.Id),
                        this.interview.GetRosterTitle(this.navigationState.CurrentGroup));

                    this.Name.ChangeText(fullRosterName);
                }
            }
        }

        public void Handle(AnswersDeclaredInvalid @event)
        {
            SendCountOfInvalidEntitiesIncreasedMessageIfNeeded();
        }

        public void Handle(StaticTextsDeclaredInvalid @event)
        {
            SendCountOfInvalidEntitiesIncreasedMessageIfNeeded();
        }

        private void SendCountOfInvalidEntitiesIncreasedMessageIfNeeded()
        {
            if (this.settings.VibrateOnError)
                this.messenger.Publish(new CountOfInvalidEntitiesIncreasedMessage(this));

        }
        
        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            var disposableItems = this.Items.OfType<IDisposable>().ToArray();

            foreach (var disposableItem in disposableItems)
            {
                disposableItem.Dispose();
            }
        }
    }
}