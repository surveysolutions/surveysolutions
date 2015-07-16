using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveySolutions.Services;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class ActiveGroupViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesTitleChanged>,
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

        private IList items;
        public IList Items
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

            eventRegistry.Subscribe(this, interviewId);
            this.navigationState = navigationState;
            this.navigationState.GroupChanged += navigationState_OnGroupChanged;
        }

        List<Guid> listOfChildrenIdOfCurrentGroup = new List<Guid>();

        public void navigationState_OnGroupChanged(GroupChangedEventArgs navigationParams)
        {
            var questionnaire = this.questionnaireRepository.GetById(this.navigationState.QuestionnaireId);

            GroupModel group = questionnaire.GroupsWithFirstLevelChildrenAsReferences[navigationParams.TargetGroup.Id];
            
            if (group is RosterModel)
            {
                string title = group.Title;
                var interview = this.interviewRepository.Get(this.navigationState.InterviewId);
                this.Name = this.substitutionService.GenerateRosterName(title, interview.GetRosterTitle(navigationParams.TargetGroup));
            }
            else
            {
                this.Name = group.Title;
            }

            var listOfViewModels = this.interviewViewModelFactory.GetEntities(
                interviewId: this.navigationState.InterviewId,
                groupIdentity: navigationParams.TargetGroup, 
                navigationState: this.navigationState);

            this.AddToParentButton(listOfViewModels, navigationParams);

            var anchoreElementIndex = 0;
            if (navigationParams.AnchoredElementIdentity != null)
            {
                var childItem = group.Children.FirstOrDefault(x => x.Id == navigationParams.AnchoredElementIdentity.Id);
                anchoreElementIndex = childItem != null ? group.Children.IndexOf(childItem) : 0;
            }

            this.Items = listOfViewModels;
            listOfChildrenIdOfCurrentGroup = group.Children.Select(x => x.Id).ToList();

            messenger.Publish(new ScrollToAnchorMessage(this, anchoreElementIndex));
        }

        private void AddToParentButton(IList listOfViewModels, GroupChangedEventArgs navigationParams)
        {
            var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
            previousGroupNavigationViewModel.Init(this.navigationState.InterviewId, navigationParams.TargetGroup, this.navigationState);
            listOfViewModels.Add(previousGroupNavigationViewModel);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            foreach (ChangedRosterInstanceTitleDto rosterInstance in @event.ChangedInstances)
            {
                if (this.navigationState.CurrentGroup.Equals(rosterInstance.RosterInstance.GetIdentity()))
                {
                    var questionnaire = this.questionnaireRepository.GetById(this.navigationState.QuestionnaireId);

                    GroupModel group = questionnaire.GroupsWithFirstLevelChildrenAsReferences[this.navigationState.CurrentGroup.Id];
                    var interview = this.interviewRepository.Get(this.navigationState.InterviewId);
                    this.Name = this.substitutionService.GenerateRosterName(@group.Title, interview.GetRosterTitle(this.navigationState.CurrentGroup));
                }
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            CalculatePositionsToUpdate(@event.Questions);
        }

        public void Handle(QuestionsDisabled @event)
        {
            CalculatePositionsToUpdate(@event.Questions);
        }

        public void Handle(GroupsEnabled @event)
        {
            CalculatePositionsToUpdate(@event.Groups);
        }

        public void Handle(GroupsDisabled @event)
        {
            CalculatePositionsToUpdate(@event.Groups);
        }

        void CalculatePositionsToUpdate(SharedKernels.DataCollection.Events.Interview.Dtos.Identity[] itemIdentities)
        {
            var positionsToUpdate = new List<int>();
            foreach (var itemIdentity in itemIdentities)
            {
                if (this.listOfChildrenIdOfCurrentGroup.Contains(itemIdentity.Id)
                    && itemIdentity.RosterVector.Identical(this.navigationState.CurrentGroup.RosterVector))
                {
                    positionsToUpdate.Add(this.listOfChildrenIdOfCurrentGroup.IndexOf(itemIdentity.Id));
                }
            }
            positionsToUpdate = positionsToUpdate.Distinct().ToList();
            positionsToUpdate.ForEach(x => this.messenger.Publish(new UpdateInterviewEntityStateMessage(this.navigationState.CurrentGroup, x)));
        }
    }
}