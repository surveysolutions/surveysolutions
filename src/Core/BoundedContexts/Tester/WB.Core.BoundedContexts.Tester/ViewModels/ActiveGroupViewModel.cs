using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveySolutions.Services;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class ActiveGroupViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<GroupsEnabled>,
        ILiteEventHandler<GroupsDisabled>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }

        private IList<IInterviewEntityViewModel> items;
        public IList<IInterviewEntityViewModel> Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }

        private readonly IInterviewViewModelFactory interviewViewModelFactory;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;
        readonly ILiteEventRegistry eventRegistry;

        private readonly IMvxMessenger messenger;

        private NavigationState navigationState;

        IStatefulInterview interview;
        QuestionnaireModel questionnaire;
        string interviewId;

        public ActiveGroupViewModel(
            IInterviewViewModelFactory interviewViewModelFactory,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            IMvxMessenger messenger)
        {
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
            this.eventRegistry = eventRegistry;
            this.messenger = messenger;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.interviewId = interviewId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.questionnaire = this.questionnaireRepository.GetById(this.interview.QuestionnaireId);

            eventRegistry.Subscribe(this, interviewId);
            this.navigationState = navigationState;
            this.navigationState.GroupChanged += navigationState_OnGroupChanged;
        }

        public void navigationState_OnGroupChanged(GroupChangedEventArgs navigationParams)
        {
            GroupModel group = questionnaire.GroupsWithFirstLevelChildrenAsReferences[navigationParams.TargetGroup.Id];

            if (group is RosterModel)
            {
                string title = group.Title;
                this.Name = this.substitutionService.GenerateRosterName(title, this.interview.GetRosterTitle(navigationParams.TargetGroup));
            }
            else
            {
                this.Name = group.Title;
            }

            this.UpdateFormModel(navigationParams.TargetGroup);

            var anchorElementIndex = 0;

            if (navigationParams.AnchoredElementIdentity != null)
            {
                var childItem = this.Items.OfType<GroupViewModel>()
                    .FirstOrDefault(x => x.Identity.Equals(navigationParams.AnchoredElementIdentity));

                anchorElementIndex = childItem != null ? this.Items.IndexOf(childItem) : 0;
            }

            this.messenger.Publish(new ScrollToAnchorMessage(this, anchorElementIndex));
        }

        void UpdateFormModel(Identity groupIdentity)
        {
            var listOfViewModels = this.interviewViewModelFactory.GetEntities(
                interviewId: this.navigationState.InterviewId,
                groupIdentity: groupIdentity,
                navigationState: this.navigationState);

            this.InsertRosterInstances(groupIdentity, listOfViewModels);
            this.AddToParentButton(groupIdentity, listOfViewModels);

            this.Items = new ObservableCollection<IInterviewEntityViewModel>(listOfViewModels);
        }

        private void AddToParentButton(Identity targetGroupIdentity, IList<IInterviewEntityViewModel> listOfViewModels)
        {
            var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
            previousGroupNavigationViewModel.Init(this.interviewId, targetGroupIdentity, this.navigationState);
            listOfViewModels.Add(previousGroupNavigationViewModel);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            foreach (ChangedRosterInstanceTitleDto rosterInstance in @event.ChangedInstances)
            {
                if (this.navigationState.CurrentGroup.Equals(rosterInstance.RosterInstance.GetIdentity()))
                {
                    GroupModel group = this.questionnaire.GroupsWithFirstLevelChildrenAsReferences[this.navigationState.CurrentGroup.Id];
                    this.Name = this.substitutionService.GenerateRosterName(@group.Title, this.interview.GetRosterTitle(this.navigationState.CurrentGroup));
                }
            }
        }

        public void Handle(RosterInstancesAdded @event)
        {
            this.UpdateFormModel(this.navigationState.CurrentGroup);
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            this.UpdateFormModel(this.navigationState.CurrentGroup);
        }

        private void InsertRosterInstances(Identity targetGroupIdentity, IList<IInterviewEntityViewModel> listOfViewModels)
        {
            GroupModel currentGroupInQuestionnaire = questionnaire.GroupsWithFirstLevelChildrenAsReferences[targetGroupIdentity.Id];

            var questionnairesRosters = currentGroupInQuestionnaire.Children.Where(questionnaireEntityReference => questionnaireEntityReference.ModelType == typeof(RosterModel));

            foreach (var questionnairesRoster in questionnairesRosters)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionnairesRoster.Id, targetGroupIdentity.RosterVector);

                if (!interview.RosterInstancesIds.ContainsKey(rosterKey))
                    return;

                this.interview.RosterInstancesIds[rosterKey].ForEach(
                    rosterInstance => this.InsertRosterInstance(rosterInstance, currentGroupInQuestionnaire, listOfViewModels));
            }
        }

        private void InsertRosterInstance(Identity rosterInstance, GroupModel currentGroupInQuestionnaire, IList<IInterviewEntityViewModel> listOfViewModels)
        {
            var questionnaireRoster = currentGroupInQuestionnaire.Children.Find(x => x.Id == rosterInstance.Id);

            var rosterInstanceIndex = currentGroupInQuestionnaire.Children.IndexOf(questionnaireRoster);

            var questionnaireEntityAfterRoster = currentGroupInQuestionnaire.Children.ElementAtOrDefault(rosterInstanceIndex + 1);

            var rosterInstanceViewModel = this.interviewViewModelFactory.GetNew<GroupViewModel>();
            rosterInstanceViewModel.Init(this.interviewId, rosterInstance, this.navigationState);

            if (questionnaireEntityAfterRoster == null) 
                listOfViewModels.Add(rosterInstanceViewModel);
            else
            {
                var interviewEntitiesAfterRosterInstance = listOfViewModels.Where(x => x.Identity != null && x.Identity.Id.Equals(questionnaireEntityAfterRoster.Id));

                rosterInstanceIndex = listOfViewModels.IndexOf(interviewEntitiesAfterRosterInstance.Last()) - 1;

                listOfViewModels.Insert(rosterInstanceIndex, rosterInstanceViewModel);
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            this.CalculatePositionsToUpdateAndNotifySubscribers(@event.Questions);
        }

        public void Handle(QuestionsDisabled @event)
        {
            this.CalculatePositionsToUpdateAndNotifySubscribers(@event.Questions);
        }

        public void Handle(GroupsEnabled @event)
        {
            this.CalculatePositionsToUpdateAndNotifySubscribers(@event.Groups);
        }

        public void Handle(GroupsDisabled @event)
        {
            this.CalculatePositionsToUpdateAndNotifySubscribers(@event.Groups);
        }

        void CalculatePositionsToUpdateAndNotifySubscribers(SharedKernels.DataCollection.Events.Interview.Dtos.Identity[] itemIdentities)
        {
            var positionsToUpdate = new List<int>();
            foreach (var itemIdentity in itemIdentities)
            {
                var interviewEntity = this.Items.FirstOrDefault(x => x.Identity != null && x.Identity.Id == itemIdentity.Id);
                if (interviewEntity != null && itemIdentity.RosterVector.Identical(this.navigationState.CurrentGroup.RosterVector))
                {
                    positionsToUpdate.Add(this.Items.IndexOf(interviewEntity));
                }
            }
            positionsToUpdate = positionsToUpdate.Distinct().ToList();
            positionsToUpdate.ForEach(x => this.messenger.Publish(new UpdateInterviewEntityStateMessage(this.navigationState.CurrentGroup, x)));
        }
    }
}