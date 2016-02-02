using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class EnumerationStageViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>,
        IDisposable
    {
        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.name = value; this.RaisePropertyChanged(); }
        }

        private ObservableRangeCollection<dynamic> items;
        public ObservableRangeCollection<dynamic> Items
        {
            get { return this.items; }
            set
            {
                this.items = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly IInterviewViewModelFactory interviewViewModelFactory;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;
        readonly ILiteEventRegistry eventRegistry;

        private readonly IMvxMessenger messenger;
        readonly IUserInterfaceStateService userInterfaceStateService;

        private NavigationState navigationState;

        IStatefulInterview interview;
        QuestionnaireModel questionnaire;
        string interviewId;


        public EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IMvxMessenger messenger,
            IUserInterfaceStateService userInterfaceStateService)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
            this.eventRegistry = eventRegistry;
            this.messenger = messenger;
            this.userInterfaceStateService = userInterfaceStateService;
        }

        public bool ShouldRemoveDisabledEntities => true;

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.interviewId = interviewId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.questionnaire = this.questionnaireRepository.GetById(this.interview.QuestionnaireId);

            this.eventRegistry.Subscribe(this, interviewId);
            this.navigationState = navigationState;
            this.Items = new ObservableRangeCollection<dynamic>();
            this.navigationState.ScreenChanged += this.OnScreenChanged;
        }

        private void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (eventArgs.TargetScreen == ScreenType.Complete)
            {
                this.Items.OfType<IDisposable>().ForEach(x => x.Dispose());
                this.Items.Clear();
            }
            else
            {
                GroupModel @group = this.questionnaire.GroupsWithFirstLevelChildrenAsReferences[eventArgs.TargetGroup.Id];

                this.CreateRegularGroupScreen(eventArgs, @group);
                if (!this.eventRegistry.IsSubscribed(this, this.interviewId))
                {
                    this.eventRegistry.Subscribe(this, this.interviewId);
                }
            }
        }

        private void CreateRegularGroupScreen(ScreenChangedEventArgs eventArgs, GroupModel @group)
        {
            if (@group is RosterModel)
            {
                string title = @group.Title;
                this.Name = this.substitutionService.GenerateRosterName(title, this.interview.GetRosterTitle(eventArgs.TargetGroup));
            }
            else
            {
                this.Name = @group.Title;
            }

            this.LoadFromModel(eventArgs.TargetGroup);
            this.SendScrollToMessage(eventArgs.AnchoredElementIdentity);
        }

        private void SendScrollToMessage(Identity scrollTo)
        {
            var anchorElementIndex = 0;

            if (scrollTo != null)
            {
                var childItem = this.Items.OfType<GroupViewModel>()
                    .FirstOrDefault(x => x.Identity.Equals(scrollTo));

                anchorElementIndex = childItem != null ? this.Items.IndexOf(childItem) : 0;
            }

            this.messenger.Publish(new ScrollToAnchorMessage(this, anchorElementIndex));
        }

        private void LoadFromModel(Identity groupIdentity)
        {
            try
            {
                userInterfaceStateService.NotifyRefreshStarted();

                var interviewEntityViewModels = this.interviewViewModelFactory.GetEntities(
                    interviewId: this.navigationState.InterviewId,
                    groupIdentity: groupIdentity,
                    navigationState: this.navigationState).ToList();


                var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
                previousGroupNavigationViewModel.Init(this.interviewId, groupIdentity, this.navigationState);

                foreach (var interviewItemViewModel in this.Items.OfType<IDisposable>())
                {
                    interviewItemViewModel.Dispose();
                }
                InvokeOnMainThread(() =>this.Items.Reset(interviewEntityViewModels.Concat(previousGroupNavigationViewModel.ToEnumerable<dynamic>())));
            }
            finally
            {
                userInterfaceStateService.NotifyRefreshFinished();
            }
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            foreach (ChangedRosterInstanceTitleDto rosterInstance in @event.ChangedInstances)
            {
                if (this.navigationState.CurrentGroup.Equals(rosterInstance.RosterInstance.GetIdentity()))
                {
                    GroupModel group = this.questionnaire.GroupsWithFirstLevelChildrenAsReferences[this.navigationState.CurrentGroup.Id];
                    this.Name = this.substitutionService.GenerateRosterName(@group.Title,
                        this.interview.GetRosterTitle(this.navigationState.CurrentGroup));
                }
            }
        }

        public void Handle(RosterInstancesAdded @event)
        {
            this.AddEntities(@event.Instances.Select(x => x.GetIdentity()).ToHashSet());
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            this.RemoveEntities(@event.Instances.Select(x => x.GetIdentity()).ToHashSet());
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (this.ShouldRemoveDisabledEntities)
            {
                this.AddEntities(@event.Questions);
            }
            else
            {
                this.InvalidateViewModelsByConditions(@event.Questions);
            }
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (this.ShouldRemoveDisabledEntities)
            {
                this.RemoveEntities(@event.Questions);
            }
            else
            {
                this.InvalidateViewModelsByConditions(@event.Questions);
            }
        }

        public void Handle(GroupsEnabled @event)
        {
            if (this.ShouldRemoveDisabledEntities)
            {
                this.AddEntities(@event.Groups);
            }
            else
            {
                this.InvalidateViewModelsByConditions(@event.Groups);
            }
        }

        public void Handle(GroupsDisabled @event)
        {
            if (this.ShouldRemoveDisabledEntities)
            {
                this.RemoveEntities(@event.Groups);
            }
            else
            {
                this.InvalidateViewModelsByConditions(@event.Groups);
            }
        }

        private void AddEntities(Identity[] identitiesToAdd) => this.AddEntities(identitiesToAdd.ToHashSet());

        private void AddEntities(HashSet<Identity> identitiesToAdd)
        {
            try
            {
                this.userInterfaceStateService.NotifyRefreshStarted();

                List<IInterviewEntityViewModel> createdViewModelEntities = this.interviewViewModelFactory.GetEntities(
                    interviewId: this.navigationState.InterviewId,
                    groupIdentity: this.navigationState.CurrentGroup,
                    navigationState: this.navigationState).ToList();

                List<IInterviewEntityViewModel> usedViewModelEntities = new List<IInterviewEntityViewModel>();

                this.InvokeOnMainThread(() =>
                {
                    for (int indexOfViewModel = 0; indexOfViewModel < createdViewModelEntities.Count; indexOfViewModel++)
                    {
                        var viewModelEntity = createdViewModelEntities[indexOfViewModel];

                        if (identitiesToAdd.Contains(viewModelEntity.Identity))
                        {
                            this.Items.Insert(indexOfViewModel, viewModelEntity);
                            usedViewModelEntities.Add(viewModelEntity);
                        }
                    }
                });

                var notUsedViewModelEntities = createdViewModelEntities.Except(usedViewModelEntities);
                notUsedViewModelEntities.OfType<IDisposable>().ForEach(x => x.Dispose());
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
            }
        }

        private void RemoveEntities(Identity[] identitiesToRemove) => this.RemoveEntities(identitiesToRemove.ToHashSet());

        private void RemoveEntities(HashSet<Identity> identitiesToRemove)
        {
            try
            {
                this.userInterfaceStateService.NotifyRefreshStarted();

                var itemsToRemove = this
                    .Items
                    .OfType<GroupViewModel>()
                    .Where(x => identitiesToRemove.Contains(x.Identity))
                    .ToList();

                itemsToRemove.ForEach(x => x.Dispose());
                this.InvokeOnMainThread(() => this.Items.RemoveRange(itemsToRemove));
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
            }
        }

        private void InvalidateViewModelsByConditions(Identity[] viewModelIdentities)
        {
            InvokeOnMainThread(() =>
            {
                var readOnlyItems = Items.ToArray();

                for (int i = 0; i < readOnlyItems.Length; i++)
                {
                    var interviewEntityViewModel = readOnlyItems[i] as IInterviewEntityViewModel;
                    if (interviewEntityViewModel != null &&
                        viewModelIdentities.Contains(interviewEntityViewModel.Identity))
                        // here inconsistency of readOnlyItems and Items collections is possible but nothing bad will happen if wrong item be marked as changed.
                        this.Items.NotifyItemChanged(i);
                }
            });
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, interviewId);
            this.navigationState.ScreenChanged -= this.OnScreenChanged;
            var disposableItems = this.Items.OfType<IDisposable>().ToArray();

            foreach (var disposableItem in disposableItems)
            {
                disposableItem.Dispose();
            }
        }
    }
}