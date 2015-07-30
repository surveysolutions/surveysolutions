using System;
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
        ILiteEventHandler<RosterInstancesRemoved>
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<IInterviewEntityViewModel> items;
        public ObservableCollection<IInterviewEntityViewModel> Items
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

            this.Items = new ObservableCollection<IInterviewEntityViewModel>();

            var listOfViewModels = this.interviewViewModelFactory.GetEntities(
                interviewId: this.navigationState.InterviewId,
                groupIdentity: navigationParams.TargetGroup,
                navigationState: this.navigationState);

            listOfViewModels.ForEach(x=>this.Items.Add(x));

            this.InsertRosterInstances(navigationParams.TargetGroup);
            this.AddToParentButton(navigationParams.TargetGroup);

            var anchorElementIndex = 0;

            if (navigationParams.AnchoredElementIdentity != null)
            {
                var childItem = this.Items.OfType<GroupViewModel>()
                    .FirstOrDefault(x => x.Identity.Equals(navigationParams.AnchoredElementIdentity));

                anchorElementIndex = childItem != null ? this.Items.IndexOf(childItem) : 0;
            }

            this.messenger.Publish(new ScrollToAnchorMessage(this, anchorElementIndex));
        }

        private void AddToParentButton(Identity targetGroupIdentity)
        {
            var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
            previousGroupNavigationViewModel.Init(this.interviewId, targetGroupIdentity, this.navigationState);
            this.Items.Add(previousGroupNavigationViewModel);
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
            foreach (var rosterInstance in @event.Instances)
            {
                if (this.questionnaire.GroupsParentIdMap[rosterInstance.GroupId] != this.navigationState.CurrentGroup.Id ||
                    !rosterInstance.OuterRosterVector.Identical(this.navigationState.CurrentGroup.RosterVector))
                    continue;

                GroupModel currentGroupInQuestionnaire = this.questionnaire.GroupsWithFirstLevelChildrenAsReferences[this.navigationState.CurrentGroup.Id];
                this.InsertRosterInstance(rosterInstance.GetIdentity(), currentGroupInQuestionnaire);
            }
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            for (int i = @event.Instances.Length - 1; i >= 0; i--)
            {
                var rosterInstance = @event.Instances[i];

                if (this.questionnaire.GroupsParentIdMap[rosterInstance.GroupId] != this.navigationState.CurrentGroup.Id ||
                    !rosterInstance.OuterRosterVector.Identical(this.navigationState.CurrentGroup.RosterVector))
                    continue;

                var rosterIdentity = rosterInstance.GetIdentity();
                var rosterInstanceViewModel =
                    this.Items.OfType<GroupViewModel>().FirstOrDefault(x => x.Identity.Equals(rosterIdentity));

                if (rosterInstanceViewModel != null)
                    this.Items.Remove(rosterInstanceViewModel);
            }
        }

        private void InsertRosterInstances(Identity targetGroupIdentity)
        {
            GroupModel currentGroupInQuestionnaire = questionnaire.GroupsWithFirstLevelChildrenAsReferences[targetGroupIdentity.Id];

            var questionnairesRosters = currentGroupInQuestionnaire.Children.Where(questionnaireEntityReference => questionnaireEntityReference.ModelType == typeof(RosterModel));

            foreach (var questionnairesRoster in questionnairesRosters)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionnairesRoster.Id, targetGroupIdentity.RosterVector);

                if (!interview.RosterInstancesIds.ContainsKey(rosterKey))
                    return;

                this.interview.RosterInstancesIds[rosterKey].ForEach(
                    rosterInstance => this.InsertRosterInstance(rosterInstance, currentGroupInQuestionnaire));
            }
        }

        private void InsertRosterInstance(Identity rosterInstance, GroupModel currentGroupInQuestionnaire)
        {
            var questionnaireRoster = currentGroupInQuestionnaire.Children.Find(x => x.Id == rosterInstance.Id);

            var rosterInstanceIndex = currentGroupInQuestionnaire.Children.IndexOf(questionnaireRoster);

            var questionnaireEntityBeforeRoster = currentGroupInQuestionnaire.Children.ElementAtOrDefault(rosterInstanceIndex - 1);

            if (questionnaireEntityBeforeRoster == null)
                rosterInstanceIndex = (int)rosterInstance.RosterVector.Last();
            else
            {
                var interviewEntitiesBeforeRosterInstance = this.Items.Where(x => x.Identity != null && x.Identity.Id.Equals(questionnaireEntityBeforeRoster.Id));

                rosterInstanceIndex = this.Items.IndexOf(interviewEntitiesBeforeRosterInstance.Last()) + 1;
            }

            var rosterInstanceViewModel = this.interviewViewModelFactory.GetNew<GroupViewModel>();
            rosterInstanceViewModel.Init(this.interviewId, rosterInstance, this.navigationState);

            this.Items.Insert(rosterInstanceIndex, rosterInstanceViewModel);
        }
    }
}