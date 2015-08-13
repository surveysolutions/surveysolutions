using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.SurveySolutions.Services;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
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
            get { return this.name; }
            set { this.name = value; this.RaisePropertyChanged(); }
        }

        private ObservableRangeCollection<dynamic> items;
        public ObservableRangeCollection<dynamic> Items
        {
            get { return this.items; }
            set { this.items = value; this.RaisePropertyChanged(); }
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

            this.eventRegistry.Subscribe(this, interviewId);
            this.navigationState = navigationState;
            this.navigationState.GroupChanged += this.navigationState_OnGroupChanged;
        }

        private async void navigationState_OnGroupChanged(GroupChangedEventArgs navigationParams)
        {
            GroupModel group = this.questionnaire.GroupsWithFirstLevelChildrenAsReferences[navigationParams.TargetGroup.Id];

            if (navigationParams.TargetGroup.Id == this.questionnaire.FinishGroupId)
            {
                this.CreateCompleteScreen(navigationParams);
            }
            else
            {
                await this.CreateRegularGroupScreen(navigationParams, @group);
            }
        }

        private void CreateCompleteScreen(GroupChangedEventArgs navigationParams)
        {
            this.Items = new ObservableRangeCollection<dynamic>();

            var completeScreenItems = this.interviewViewModelFactory
                .GetCompleteScreenEntities(this.navigationState.InterviewId);
            
            completeScreenItems.ForEach(x => this.Items.Add(x)); ;
            this.Name = UIResources.Interview_Complete_Screen_Title;
        }


        private async Task CreateRegularGroupScreen(GroupChangedEventArgs navigationParams, GroupModel @group)
        {
            if (@group is RosterModel)
            {
                string title = @group.Title;
                this.Name = this.substitutionService.GenerateRosterName(
                    title,
                    this.interview.GetRosterTitle(navigationParams.TargetGroup));
            }
            else
            {
                this.Name = @group.Title;
            }

            await Task.Run(
                () =>
                {
                    this.LoadFromModel(navigationParams.TargetGroup);
                    this.SendScrollToMessage(navigationParams.AnchoredElementIdentity);
                });
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
            this.Items = new ObservableRangeCollection<dynamic>();

            this.interviewViewModelFactory.GetEntities(
                interviewId: this.navigationState.InterviewId,
                groupIdentity: groupIdentity,
                navigationState: this.navigationState).ForEach(x => this.Items.Add(x));

            var previousGroupNavigationViewModel = this.interviewViewModelFactory.GetNew<GroupNavigationViewModel>();
            previousGroupNavigationViewModel.Init(this.interviewId, groupIdentity, this.navigationState);
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
            var viewModelEntities = this.interviewViewModelFactory.GetEntities(
                interviewId: this.navigationState.InterviewId,
                groupIdentity: this.navigationState.CurrentGroup,
                navigationState: this.navigationState).ToList();

            foreach (var addedRosterInstance in @event.Instances)
            {
                var viewModelEntity = viewModelEntities.FirstOrDefault(x => x.Identity.Equals(addedRosterInstance.GetIdentity()));

                if (viewModelEntity != null)
                    this.Items.Insert(viewModelEntities.IndexOf(viewModelEntity), viewModelEntity);
            }
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            var itemsToRemove = this.Items.OfType<IInterviewEntityViewModel>().Where(x => @event.Instances.Any(y => x.Identity.Equals(y.GetIdentity())));

            this.Items.RemoveRange(itemsToRemove);
        }

        public void Handle(QuestionsEnabled @event)
        {
            this.InvalidateViewModelsByConditions(@event.Questions.ToIdentities());
        }

        public void Handle(QuestionsDisabled @event)
        {
            this.InvalidateViewModelsByConditions(@event.Questions.ToIdentities());
        }

        public void Handle(GroupsEnabled @event)
        {
            this.InvalidateViewModelsByConditions(@event.Groups.ToIdentities());
        }

        public void Handle(GroupsDisabled @event)
        {
            this.InvalidateViewModelsByConditions(@event.Groups.ToIdentities());
        }

        void InvalidateViewModelsByConditions(IEnumerable<Identity> viewModelIdentities)
        {
            foreach (var viewModelIdentity in viewModelIdentities)
            {
                var interviewEntity = this.Items.OfType<IInterviewEntityViewModel>().FirstOrDefault(x => x.Identity.Equals(viewModelIdentity));

                if (interviewEntity != null)
                    this.Items[this.Items.IndexOf(interviewEntity)] = interviewEntity;
            }
        }
    }
}