﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
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
        IDisposable
    {
        private string name;
        public string Name
        {
            get { return this.name; }
            set { this.name = value; this.RaisePropertyChanged(); }
        }

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
        readonly ILiteEventRegistry eventRegistry;

        readonly IUserInterfaceStateService userInterfaceStateService;
        private readonly IMvxMainThreadDispatcher mvxMainThreadDispatcher;

        private readonly object itemsListUpdateOnUILock = new object();

        private NavigationState navigationState;

        IStatefulInterview interview;
        private IQuestionnaire questionnaire;
        string interviewId;


        public EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IUserInterfaceStateService userInterfaceStateService,
            IMvxMainThreadDispatcher mvxMainThreadDispatcher)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
            this.eventRegistry = eventRegistry;
            this.userInterfaceStateService = userInterfaceStateService;
            this.mvxMainThreadDispatcher = mvxMainThreadDispatcher;
        }

        public async Task InitAsync(string interviewId, NavigationState navigationState, Identity groupId, Identity anchoredElementIdentity)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.interviewId = interviewId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity);

            this.navigationState = navigationState;
            this.Items = new ObservableRangeCollection<IInterviewEntityViewModel>();

            await this.CreateRegularGroupScreenAsync(groupId, anchoredElementIdentity);

            if (!this.eventRegistry.IsSubscribed(this, this.interviewId))
            {
                this.eventRegistry.Subscribe(this, this.interviewId);
            }
        }

        private async Task CreateRegularGroupScreenAsync(Identity groupId, Identity anchoredElementIdentity)
        {
            if (this.questionnaire.IsRosterGroup(groupId.Id))
            {
                string title = this.questionnaire.GetGroupTitle(groupId.Id);
                this.Name = this.substitutionService.GenerateRosterName(title, this.interview.GetRosterTitle(groupId));
            }
            else
            {
                this.Name = this.questionnaire.GetGroupTitle(groupId.Id); ;
            }

            await this.LoadFromModelAsync(groupId);
            this.SetScrollTo(anchoredElementIdentity);
        }

        private void SetScrollTo(Identity scrollTo)
        {
            var anchorElementIndex = 0;

            if (scrollTo != null)
            {
                var childItem = this.Items.OfType<GroupViewModel>()
                    .FirstOrDefault(x => x.Identity.Equals(scrollTo));

                anchorElementIndex = childItem != null ? this.Items.IndexOf(childItem) : 0;
            }
            this.ScrollToIndex = anchorElementIndex;
        }

        public int? ScrollToIndex { get; set; }

        private async Task LoadFromModelAsync(Identity groupIdentity)
        {
            try
            { 
                this.userInterfaceStateService.NotifyRefreshStarted();

                var entities = await this.interviewViewModelFactory
                    .GetEntitiesAsync(
                        interviewId: this.navigationState.InterviewId,
                        groupIdentity: groupIdentity,
                        navigationState: this.navigationState);

                var interviewEntityViewModels = entities
                    .Where(entity => !this.ShouldBeHidden(entity.Identity))
                    .ToList();


                var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
                await previousGroupNavigationViewModel.InitAsync(this.interviewId, groupIdentity, this.navigationState);

                foreach (var interviewItemViewModel in this.Items.OfType<IDisposable>())
                {
                    interviewItemViewModel.Dispose();
                }
                this.mvxMainThreadDispatcher.RequestMainThreadAction(() => this.Items.Reset(interviewEntityViewModels.Concat(previousGroupNavigationViewModel.ToEnumerable<IInterviewEntityViewModel>())));
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
            }
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            foreach (ChangedRosterInstanceTitleDto rosterInstance in @event.ChangedInstances)
            {
                if (this.navigationState.CurrentGroup.Equals(rosterInstance.RosterInstance.GetIdentity()))
                {
                    this.Name = this.substitutionService.GenerateRosterName(
                        this.questionnaire.GetGroupTitle(this.navigationState.CurrentGroup.Id),
                        this.interview.GetRosterTitle(this.navigationState.CurrentGroup));
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
            this.RemoveEntities(@event.StaticTexts.Where(this.ShouldBeHiddenIfDisabled).ToArray());
        }

        public void Handle(StaticTextsEnabled @event)
        {
            this.AddMissingEntities();
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
                                .GetEntitiesAsync(
                                    interviewId: this.navigationState.InterviewId,
                                    groupIdentity: this.navigationState.CurrentGroup,
                                    navigationState: this.navigationState));

                        List<IInterviewEntityViewModel> createdViewModelEntities = entities
                            .Where(entity => !this.ShouldBeHidden(entity.Identity))
                            .ToList();

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
            InvokeOnMainThread(() =>
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
            this.eventRegistry.Unsubscribe(this, interviewId);
            var disposableItems = this.Items.OfType<IDisposable>().ToArray();

            foreach (var disposableItem in disposableItems)
            {
                disposableItem.Dispose();
            }
        }
    }
}