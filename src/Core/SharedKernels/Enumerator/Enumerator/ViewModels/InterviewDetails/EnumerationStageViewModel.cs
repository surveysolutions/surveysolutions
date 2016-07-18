﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using Nito.AsyncEx;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class EnumerationStageViewModel : MvxViewModel,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>,
        ILiteEventHandler<StaticTextsDisabled>,
        ILiteEventHandler<StaticTextsEnabled>,
        ILiteEventHandler<AnswersDeclaredInvalid>,
        ILiteEventHandler<StaticTextsDeclaredInvalid>,
        IDisposable
    {
        private ObservableRangeCollection<IInterviewEntityViewModel> items;
        public ObservableRangeCollection<IInterviewEntityViewModel> Items
        {
            get { return this.items; }
            set
            {
                this.items = value;
                this.RaisePropertyChanged();
            }
        }

        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IEnumeratorSettings settings;
        readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMessenger messenger;

        readonly IUserInterfaceStateService userInterfaceStateService;
        private readonly IMvxMainThreadDispatcher mvxMainThreadDispatcher;

        private readonly object itemsListUpdateOnUILock = new object();

        private NavigationState navigationState;

        IStatefulInterview interview;
        private IQuestionnaire questionnaire;
        string interviewId;

        public DynamicTextViewModel Name { get; }

        public EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxMainThreadDispatcher mvxMainThreadDispatcher,
            DynamicTextViewModel dynamicTextViewModel, IMvxMessenger messenger, IEnumeratorSettings settings)
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
        }

        public void Init(string interviewId, NavigationState navigationState, Identity groupId, Identity anchoredElementIdentity)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (this.navigationState != null) throw new InvalidOperationException("ViewModel already initialized");

            this.interviewId = interviewId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language);

            this.navigationState = navigationState;
            this.Items = new ObservableRangeCollection<IInterviewEntityViewModel>();

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
                    var childItem = this.Items
                        .FirstOrDefault(x => x.Identity.Equals(scrollTo));

                    anchorElementIndex = childItem != null ? this.Items.IndexOf(childItem) : 0;
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

                var entities = this.interviewViewModelFactory
                    .GetEntities(
                        interviewId: this.navigationState.InterviewId,
                        groupIdentity: groupIdentity,
                        navigationState: this.navigationState);

                var interviewEntityViewModels = entities
                    .Where(entity => !this.ShouldBeHidden(entity.Identity))
                    .ToList();

                var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
                previousGroupNavigationViewModel.Init(this.interviewId, groupIdentity, this.navigationState);

                foreach (var interviewItemViewModel in this.Items.OfType<IDisposable>())
                {
                    interviewItemViewModel.Dispose();
                }
                this.mvxMainThreadDispatcher.RequestMainThreadAction(() => 
                    this.Items.Reset(interviewEntityViewModels.Concat(previousGroupNavigationViewModel.ToEnumerable<IInterviewEntityViewModel>())));
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
            }
        }

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

        public void Handle(RosterInstancesAdded @event)
        {
            this.AddMissingEntities();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            this.RemoveEntities(@event.Instances.Select(x => x.GetIdentity()).ToHashSet());
        }

        public void Handle(QuestionsEnabled @event)
        {
            this.AddMissingEntities();
            this.InvalidateViewModelsByConditions(@event.Questions);
        }

        public void Handle(QuestionsDisabled @event)
        {
            this.InvalidateViewModelsByConditions(@event.Questions);
            this.RemoveEntities(@event.Questions.Where(this.ShouldBeHiddenIfDisabled).ToArray());
        }

        public void Handle(GroupsEnabled @event)
        {
            this.AddMissingEntities();
            this.InvalidateViewModelsByConditions(@event.Groups);
        }

        public void Handle(GroupsDisabled @event)
        {
            this.InvalidateViewModelsByConditions(@event.Groups);
            this.RemoveEntities(@event.Groups.Where(this.ShouldBeHiddenIfDisabled).ToArray());
        }

        public void Handle(StaticTextsDisabled @event)
        {
            this.InvalidateViewModelsByConditions(@event.StaticTexts);
            this.RemoveEntities(@event.StaticTexts.Where(this.ShouldBeHiddenIfDisabled).ToArray());
        }

        public void Handle(StaticTextsEnabled @event)
        {
            this.AddMissingEntities();
            this.InvalidateViewModelsByConditions(@event.StaticTexts);
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

        private void AddMissingEntities()
        {
            this.mvxMainThreadDispatcher.RequestMainThreadAction(() =>
            {
                lock (this.itemsListUpdateOnUILock)
                {
                    try
                    {
                        this.userInterfaceStateService.NotifyRefreshStarted();

                        var entities = AsyncContext.Run(() =>
                             this.interviewViewModelFactory
                                .GetEntities(
                                    interviewId: this.navigationState.InterviewId,
                                    groupIdentity: this.navigationState.CurrentGroup,
                                    navigationState: this.navigationState)).ToList();

                        List<IInterviewEntityViewModel> createdViewModelEntities = entities
                            .Where(entity => !this.ShouldBeHidden(entity.Identity))
                            .ToList();

                        var notUsedEntities = entities.Except(createdViewModelEntities);
                        notUsedEntities.OfType<IDisposable>().ForEach(x => x.Dispose());

                        List<IInterviewEntityViewModel> usedViewModelEntities = new List<IInterviewEntityViewModel>();

                        for (int indexOfViewModel = 0; indexOfViewModel < createdViewModelEntities.Count; indexOfViewModel++)
                        {
                            var viewModelEntity = createdViewModelEntities[indexOfViewModel];

                            var existingIdentities =
                                this.Items
                                    .Select(entity => entity.Identity)
                                    .ToHashSet();

                            if (!existingIdentities.Contains(viewModelEntity.Identity))
                            {
                                this.Items.Insert(indexOfViewModel, viewModelEntity);
                                usedViewModelEntities.Add(viewModelEntity);
                            }
                        }

                        var notUsedViewModelEntities = createdViewModelEntities.Except(usedViewModelEntities);
                        notUsedViewModelEntities.OfType<IDisposable>().ForEach(x => x.Dispose());
                    }
                    finally
                    {
                        this.userInterfaceStateService.NotifyRefreshFinished();
                    }
                }
            });
        }

        private void RemoveEntities(Identity[] identitiesToRemove) => this.RemoveEntities(identitiesToRemove.ToHashSet());

        private void RemoveEntities(HashSet<Identity> identitiesToRemove)
        {
            this.mvxMainThreadDispatcher.RequestMainThreadAction(() =>
            {
                lock (this.itemsListUpdateOnUILock)
                {
                    try
                    {
                        this.userInterfaceStateService.NotifyRefreshStarted();

                        var itemsToRemove = this
                            .Items
                            .Where(item => identitiesToRemove.Contains(item.Identity))
                            .ToList();

                        this.Items.RemoveRange(itemsToRemove);

                        foreach (var item in itemsToRemove.OfType<IDisposable>())
                        {
                            item.Dispose();
                        }
                    }
                    finally
                    {
                        this.userInterfaceStateService.NotifyRefreshFinished();
                    }
                }
            });
        }

        private void InvalidateViewModelsByConditions(Identity[] viewModelIdentities)
        {
            this.mvxMainThreadDispatcher.RequestMainThreadAction(() =>
            {
                var readOnlyItems = this.Items.ToArray();

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

        private bool ShouldBeHidden(Identity entity)
            => this.ShouldBeHiddenIfDisabled(entity) &&
               !this.interview.IsEnabled(entity);

        private bool ShouldBeHiddenIfDisabled(Identity entity)
            => this.questionnaire.ShouldBeHiddenIfDisabled(entity.Id);

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