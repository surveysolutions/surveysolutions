using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    [DebuggerDisplay("Title = {Title}, Id = {SectionIdentity}")]
    public class SideBarSectionViewModel : MvxNotifyPropertyChanged,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        IDisposable
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IMvxMessenger messenger;

        public DynamicTextViewModel Title { get; }

        private string interviewId;
        QuestionnaireIdentity questionnaireId;

        private SideBarSectionsViewModel root;

        public SideBarSectionViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            ISubstitutionService substitutionService,
            ILiteEventRegistry eventRegistry,
            ISideBarSectionViewModelsFactory modelsFactory,
            IMvxMessenger messenger,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionService = substitutionService;
            this.eventRegistry = eventRegistry;
            this.modelsFactory = modelsFactory;
            this.messenger = messenger;

            this.Title = dynamicTextViewModel;

            this.Children = new List<SideBarSectionViewModel>();
        }

        public void Init(string interviewId,
            NavigationIdentity navigationIdentity,
            SideBarSectionsViewModel root,
            SideBarSectionViewModel parent,
            GroupStateViewModel groupStateViewModel,
            NavigationState navigationState)
        {
            this.interviewId = interviewId;

            this.eventRegistry.Subscribe(this, interviewId);
            
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            this.questionnaireId = interview.QuestionnaireIdentity;
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            groupStateViewModel.Init(interviewId, navigationIdentity.TargetGroup);
            this.root = root;
            this.Parent = parent;
            this.SectionIdentity = navigationIdentity.TargetGroup;
            this.HasChildren = interview.GetEnabledSubgroups(navigationIdentity.TargetGroup).Any();
            this.NodeDepth = this.UnwrapReferences(x => x.Parent).Count() - 1;
            this.IsCurrent = this.SectionIdentity.Equals(navigationState.CurrentGroup);
            var groupId = navigationIdentity.TargetGroup.Id;
            var groupTitle = questionnaire.GetGroupTitle(groupId);
            if (questionnaire.IsRosterGroup(groupId))
            {
                string rosterTitle = interview.GetRosterTitle(navigationIdentity.TargetGroup);
                var rosterFullName = this.substitutionService.GenerateRosterName(groupTitle, rosterTitle);
                this.Title.Init(interviewId, this.SectionIdentity, rosterFullName);
            }
            else
            {
                this.Title.Init(interviewId, this.SectionIdentity, groupTitle);
            }
            if (this.Parent != null)
            {
                this.IsSelected = this.Parent.IsSelected;
            }
           
            this.SideBarGroupState = groupStateViewModel;
            this.ScreenType = navigationIdentity.TargetScreen;
            this.NavigationState = navigationState;
            this.NavigationState.ScreenChanged += this.OnScreenChanged;
            this.NavigationState.BeforeScreenChanged += this.OnBeforeScreenChanged;
        }

        // remove code duplication
        public void InitCompleteScreenItem(string interviewId, GroupStateViewModel groupStateViewModel, NavigationState navigationState)
        {
            this.interviewId = interviewId;

            this.eventRegistry.Subscribe(this, interviewId);
            
            this.Parent = null;
            this.HasChildren = false;
            this.NodeDepth = 0;
            this.IsCurrent = navigationState.CurrentScreenType == ScreenType.Complete;
            this.Title.InitAsStatic(UIResources.Interview_Complete_Screen_Title);
            groupStateViewModel.Init(interviewId, null);

            this.SideBarGroupState = groupStateViewModel;
            this.ScreenType = ScreenType.Complete;
            this.NavigationState = navigationState;
            this.NavigationState.ScreenChanged += this.OnScreenChanged;
            this.NavigationState.BeforeScreenChanged += this.OnBeforeScreenChanged;
        }


        public void InitCoverScreenItem(string interviewId, CoverStateViewModel coverStateViewModel, NavigationState navigationState)
        {
            this.interviewId = interviewId;

            this.eventRegistry.Subscribe(this, interviewId);

            this.Parent = null;
            this.HasChildren = false;
            this.NodeDepth = 0;
            this.IsCurrent = navigationState.CurrentScreenType == ScreenType.Cover;
            this.Title.InitAsStatic(UIResources.Interview_Cover_Screen_Title);
            coverStateViewModel.Init(interviewId, null);

            this.SideBarGroupState = coverStateViewModel;
            this.ScreenType = ScreenType.Cover;
            this.NavigationState = navigationState;
            this.NavigationState.ScreenChanged += this.OnScreenChanged;
            this.NavigationState.BeforeScreenChanged += this.OnBeforeScreenChanged;
        }

        void OnBeforeScreenChanged(BeforeScreenChangedEventArgs eventArgs)
        {
            this.IsCurrent = false;
        }

        void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (this.ScreenType != eventArgs.TargetScreen)
                return;

            switch (eventArgs.TargetScreen)
            {
                case ScreenType.Complete:
                case ScreenType.Cover:
                    this.IsCurrent = true;
                    break;
                default:
                    if (this.SectionIdentity.Equals(eventArgs.TargetGroup))
                    {
                        this.IsCurrent = true;
                        if (!this.Expanded)
                        {
                            this.Expanded = true;
                        }
                    }
                    break;
            }
        }

        public GroupStateViewModel SideBarGroupState { get; private set; }

        public NavigationState NavigationState { get; set; }
        public Identity SectionIdentity { get; set; }
        public ScreenType ScreenType { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected == value) return;
                this.isSelected = value;
                this.RaisePropertyChanged();
            }
        }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return this.isCurrent; }
            set
            {
                if (this.isCurrent == value) return;
                this.isCurrent = value;
                this.RaisePropertyChanged();
            }
        }

        private bool hasChildren;
        public bool HasChildren
        {
            get { return this.hasChildren; }
            set
            {
                if (this.hasChildren == value) return;
                this.hasChildren = value;
                this.RaisePropertyChanged();
            }
        }

        private bool expanded;
        public bool Expanded
        {
            get { return this.expanded; }
            set
            {
                if (this.Expanded != value)
                {
                    this.expanded = value;
                    if (this.expanded)
                    {
                        this.Children = this.GenerateChildNodes();
                    }
                    else
                    {
                        this.Children.TreeToEnumerable(x => x.Children)
                                .ToList()
                                .ForEach(x => x.Dispose());

                        this.Children = new List<SideBarSectionViewModel>();
                    }

                    this.RaisePropertyChanged();
                }
            }
        }

        public int NodeDepth { get; set; }

        private List<SideBarSectionViewModel> children;
        public List<SideBarSectionViewModel> Children
        {
            get { return this.children; }
            set
            {
                this.children = value;
                this.RaisePropertyChanged();
            }
        }
        public ICommand NavigateToSectionCommand => new MvxCommand(this.NavigateToSection);

        public ICommand Toggle
        {
            get
            {
                return new MvxCommand(() =>
                {
                    this.Expanded = !this.Expanded;
                    this.root.UpdateSideBarTree();
                });
            }
        }

        private void NavigateToSection()
        {
            this.messenger.Publish(new SectionChangeMessage(this));

            NavigationIdentity navigationIdentity;
            switch (this.ScreenType)
            {
                case ScreenType.Complete:
                    navigationIdentity = NavigationIdentity.CreateForCompleteScreen();
                    break;
                case ScreenType.Cover:
                    navigationIdentity = NavigationIdentity.CreateForCoverScreen();
                    break;
                default:
                    navigationIdentity = NavigationIdentity.CreateForGroup(this.SectionIdentity);
                    break;
            }

            this.NavigationState.NavigateTo(navigationIdentity);
        }

        private List<SideBarSectionViewModel> GenerateChildNodes()
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(this.NavigationState.InterviewId);

            var result = interview.GetEnabledSubgroups(this.SectionIdentity)
                                  .Select(groupInstance => this.modelsFactory.BuildSectionItem(this.root, this, NavigationIdentity.CreateForGroup(groupInstance), this.NavigationState, this.NavigationState.InterviewId));

            return new List<SideBarSectionViewModel>(result);
        }

        public SideBarSectionViewModel Parent { get; set; }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            if (ScreenType == ScreenType.Complete || ScreenType == ScreenType.Cover)
                return;

            var myChangedInstance = @event.ChangedInstances.SingleOrDefault(x => x.RosterInstance.GetIdentity().Equals(this.SectionIdentity));
            if (myChangedInstance != null)
            {
                var interview = this.statefulInterviewRepository.Get(this.interviewId);
                IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(this.questionnaireId, interview.Language);
                if (questionnaire == null) throw new Exception("questionnaire is null");

                string groupTitle = questionnaire.GetGroupTitle(this.SectionIdentity.Id);
                string rosterTitle = myChangedInstance.Title;

                string rosterFullName = this.substitutionService.GenerateRosterName(groupTitle, rosterTitle);
                this.Title.ChangeText(rosterFullName);
            }
        }

        public void RemoveMe()
        {
            this.eventRegistry.Unsubscribe(this);
            this.RefreshHasChildrenFlag();
            this.Dispose();
        }

        public void Dispose()
        {
            this.NavigationState.ScreenChanged -= this.OnScreenChanged;
            this.NavigationState.BeforeScreenChanged -= this.OnBeforeScreenChanged;
        }

        public void RefreshHasChildrenFlag()
        {
            if (ScreenType == ScreenType.Complete || ScreenType == ScreenType.Cover)
                return;

            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            this.HasChildren = interview.GetEnabledSubgroups(this.SectionIdentity).Any();
        }
    }
}